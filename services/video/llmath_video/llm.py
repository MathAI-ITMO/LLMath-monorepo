from __future__ import annotations

import json
import os
import re
import shutil
import subprocess
import time
from datetime import datetime
from typing import Callable, Dict, List, Sequence
from openai import OpenAI
import av

from config_manager import get_llm_setting, get_prompt_template

try:
    import whisper
except Exception:
    whisper = None


def get_openai_client(
    llm_config: Dict, *, base_key: str = "openai_api_base", key_name: str = "openai_api_key"
):
    if OpenAI is None:
        raise RuntimeError("openai package is not installed")
    base_url = get_llm_setting(llm_config, base_key)
    api_key = (llm_config.get(key_name) or llm_config.get("openai_api_key") or "").strip()
    if not api_key:
        raise RuntimeError(f"{key_name} is not configured")
    return OpenAI(
        api_key=api_key,
        base_url=base_url,
    )


def call_openai_text(client, model: str, input_text: str) -> str:
    last_err = None
    for attempt in range(3):
        try:
            chat = client.chat.completions.create(
                model=model,
                messages=[{"role": "user", "content": input_text}],
            )
            return (chat.choices[0].message.content or "").strip()
        except Exception as e:
            last_err = e
            msg = str(e)
            if "429" in msg or "rate" in msg.lower():
                time.sleep(1.5 * (attempt + 1))
                continue
            break
    if last_err:
        raise last_err
    return ""


def _probe_duration(audio_path: str, base_dir: str) -> float:
    container = av.open(audio_path)
    try:
        if container.duration is not None:
            return float(container.duration) / av.time_base
        for stream in container.streams:
            if stream.type == "audio" and stream.duration and stream.time_base:
                return float(stream.duration * stream.time_base)
        return 0.0
    finally:
        container.close()


def _fallback_segments(full_text: str, base_dir: str, audio_path: str) -> list:
    full_text = (full_text or "").strip()
    if not full_text:
        return []
    sentences = [
        s.strip()
        for s in re.split(r"(?<=[.!?])\s+", full_text)
        if s.strip()
    ]
    if not sentences:
        sentences = [full_text]
    dur = _probe_duration(audio_path, base_dir) or 0.0
    n = len(sentences)
    segs = []
    if dur <= 0.0:
        t = 0.0
        for s in sentences:
            segs.append({"start": t, "end": t, "text": s})
        return segs
    step = dur / max(1, n)
    t = 0.0
    for s in sentences:
        start = t
        end = min(dur, start + step)
        segs.append({"start": float(start), "end": float(end), "text": s})
        t = end
    return segs


CHUNK_SECONDS = 600  # 10-minute chunks


def _split_audio_chunks(audio_path: str, chunk_seconds: int) -> list:
    """Split mp3 into fixed-length chunks, return list of (offset_sec, tmp_path)."""
    import tempfile
    from av.audio.resampler import AudioResampler
    chunks = []
    in_container = av.open(audio_path)
    try:
        audio_stream = next((s for s in in_container.streams if s.type == "audio"), None)
        if audio_stream is None:
            return []
        sample_rate = audio_stream.sample_rate or 16000
        layout = str(audio_stream.layout) if hasattr(audio_stream, "layout") else "mono"
        chunk_index = 0
        offset_sec = 0.0
        out_container = None
        out_stream = None
        tmp_path = None
        resampler = AudioResampler(format="s16", layout="mono", rate=16000)
        samples_per_chunk = chunk_seconds * 16000
        samples_written = 0

        def _new_chunk(idx: int):
            tmp = tempfile.NamedTemporaryFile(suffix=".mp3", delete=False)
            tmp.close()
            oc = av.open(tmp.name, mode="w")
            os = oc.add_stream("mp3", rate=16000)
            os.layout = "mono"
            os.bit_rate = 48000
            return tmp.name, oc, os

        tmp_path, out_container, out_stream = _new_chunk(chunk_index)

        for packet in in_container.demux(audio_stream):
            for frame in packet.decode():
                resampled = resampler.resample(frame)
                if resampled is None:
                    continue
                frames = resampled if isinstance(resampled, list) else [resampled]
                for rf in frames:
                    for out_packet in out_stream.encode(rf):
                        out_container.mux(out_packet)
                    samples_written += rf.samples
                    if samples_written >= samples_per_chunk:
                        for out_packet in out_stream.encode(None):
                            out_container.mux(out_packet)
                        out_container.close()
                        chunks.append((offset_sec, tmp_path))
                        offset_sec += samples_written / 16000
                        chunk_index += 1
                        samples_written = 0
                        tmp_path, out_container, out_stream = _new_chunk(chunk_index)

        for out_packet in out_stream.encode(None):
            out_container.mux(out_packet)
        out_container.close()
        if samples_written > 0:
            chunks.append((offset_sec, tmp_path))
        else:
            os_module = __import__("os")
            try:
                os_module.unlink(tmp_path)
            except Exception:
                pass
    finally:
        in_container.close()
    return chunks


def _parse_resp_segments(resp) -> tuple:
    """Return (segments_list, full_text) from a transcription response."""
    raw_segments = getattr(resp, "segments", None)
    if raw_segments is None:
        try:
            raw_segments = resp.get("segments")
        except Exception:
            raw_segments = None
    segments = []
    for seg in (raw_segments or []):
        try:
            start = float(seg.get("start"))
            end = float(seg.get("end"))
            text = (seg.get("text") or "").strip()
        except Exception:
            start = float(getattr(seg, "start", 0.0))
            end = float(getattr(seg, "end", 0.0))
            text = (getattr(seg, "text", "") or "").strip()
        if text:
            segments.append({"start": start, "end": end, "text": text})
    full_text = (getattr(resp, "text", "") or "").strip()
    if not full_text:
        try:
            full_text = (resp.get("text") or "").strip()
        except Exception:
            full_text = ""
    return segments, full_text


def _transcribe_chunk(client, stt_model: str, language: str, chunk_path: str, offset: float) -> list:
    """Transcribe one chunk file and return segments shifted by offset."""
    with open(chunk_path, "rb") as f:
        resp = client.audio.transcriptions.create(
            model=stt_model,
            file=f,
            response_format="verbose_json",
            language=language,
            timestamp_granularities=["segment"],
        )
    segs, full_text = _parse_resp_segments(resp)
    if segs:
        return [{"start": s["start"] + offset, "end": s["end"] + offset, "text": s["text"]} for s in segs]
    if not full_text:
        return []
    dur = _probe_duration(chunk_path, "")
    fallback = _fallback_segments(full_text, "", chunk_path)
    return [{"start": s["start"] + offset, "end": s["end"] + offset, "text": s["text"]} for s in fallback]


def transcribe_with_openai(audio_path: str, llm_config: Dict, base_dir: str):
    client = get_openai_client(
        llm_config, base_key="openai_stt_api_base", key_name="openai_stt_api_key"
    )
    stt_model = get_llm_setting(llm_config, "openai_stt_model")
    language = get_llm_setting(llm_config, "whisper_language")

    chunks = _split_audio_chunks(audio_path, CHUNK_SECONDS)
    if not chunks:
        # file fits in one shot or splitting failed — try directly
        chunks = [(0.0, audio_path)]

    all_segments = []
    tmp_paths = [p for _, p in chunks if p != audio_path]
    try:
        for offset, chunk_path in chunks:
            segs = _transcribe_chunk(client, stt_model, language, chunk_path, offset)
            all_segments.extend(segs)
    finally:
        import os as _os
        for p in tmp_paths:
            try:
                _os.unlink(p)
            except Exception:
                pass

    return all_segments


def transcribe_with_whisper_local(audio_path: str, llm_config: Dict, base_dir: str):
    """
    Local transcription using openai-whisper python package.
    Returns list of {start, end, text} segment dicts or [] on failure.
    """
    try:
        model_name = get_llm_setting(llm_config, "whisper_local_model") or "base"
        language = (get_llm_setting(llm_config, "whisper_language") or "").strip() or None
        model = whisper.load_model(model_name)
        result = model.transcribe(audio_path, language=language)
        segments: List[dict] = []
        for seg in (result.get("segments") or []):
            text = (seg.get("text") or "").strip()
            if not text:
                continue
            try:
                start = float(seg.get("start", 0.0))
                end = float(seg.get("end", 0.0))
            except Exception:
                start = float(seg.get("start") or 0.0) if hasattr(seg, "get") else 0.0
                end = float(seg.get("end") or 0.0) if hasattr(seg, "get") else 0.0
            segments.append({"start": start, "end": end, "text": text})
        if segments:
            return segments
        full_text = (result.get("text") or "").strip()
        if not full_text:
            return []
        return _fallback_segments(full_text, base_dir, audio_path)
    except Exception:
        return []


def transcribe_audio(audio_path: str, llm_config: Dict, base_dir: str):
    mode = (get_llm_setting(llm_config, "stt_mode") or "api").strip().lower()
    if mode == "local":
        return transcribe_with_whisper_local(audio_path, llm_config, base_dir)
    return transcribe_with_openai(audio_path, llm_config, base_dir)


def summarize_with_llm(
    text: str,
    filename: str,
    llm_config: Dict,
    config: Dict,
    logger: Callable[[str, dict], None],
) -> str:
    api_key = llm_config.get("openai_api_key")
    if not api_key or OpenAI is None:
        return ""
    try:
        client = get_openai_client(llm_config)
        model = get_llm_setting(llm_config, "openai_model")
        prompt_tpl = get_prompt_template(config, "summary")
        input_text = prompt_tpl.replace("{transcript}", text)
        logger(
            filename,
            {
                "type": "summary_request",
                "time": datetime.now().isoformat(timespec="seconds"),
                "model": model,
                "content": prompt_tpl,
            },
        )
        summary = call_openai_text(client, model, input_text)
        if summary:
            logger(
                filename,
                {
                    "type": "summary_response",
                    "time": datetime.now().isoformat(timespec="seconds"),
                    "content": summary,
                },
            )
        return summary
    except Exception as e:
        logger(
            filename,
            {
                "type": "error",
                "time": datetime.now().isoformat(timespec="seconds"),
                "content": f"summary_error: {e}",
            },
        )
        return ""


def build_timecoded_transcript(segments: Sequence[dict]) -> str:
    def hhmmss(sec: float) -> str:
        s = int(sec)
        h = s // 3600
        m = (s % 3600) // 60
        ss = s % 60
        return f"{h:02d}:{m:02d}:{ss:02d}"

    lines = []
    for seg in segments or []:
        start = float(seg.get("start", 0))
        text = (seg.get("text") or "").strip()
        if text:
            lines.append(f"[{hhmmss(start)}] {text}")
    return "\n".join(lines)


def generate_suggestions_with_llm(
    timecoded_transcript: str,
    filename: str,
    subs_count: int,
    llm_config: Dict,
    config: Dict,
    logger: Callable[[str, dict], None],
) -> List[dict]:
    api_key = llm_config.get("openai_api_key")
    if not api_key or OpenAI is None:
        return []
    tpl = get_prompt_template(config, "suggestions")

    def hhmmss_from_sec(sec: int) -> str:
        sec = max(0, int(sec or 0))
        h = sec // 3600
        m = (sec % 3600) // 60
        s = sec % 60
        return f"{h:02d}:{m:02d}:{s:02d}"

    min_dur_sec = int((config.get("suggestions_min_duration_sec") or 60))
    min_words = int((config.get("suggestions_min_words") or 3))
    max_words = int((config.get("suggestions_max_words") or 6))
    div = int((config.get("suggestions_min_count_divider") or 20))
    extra = int((config.get("suggestions_min_count_extra") or 10))
    try:
        min_count = int((subs_count or 0) // max(1, div)) + extra
    except Exception:
        min_count = extra

    user_prompt = (
        tpl.replace("{timecoded_transcript}", timecoded_transcript)
        .replace("{min_duration}", hhmmss_from_sec(min_dur_sec))
        .replace("{min_count}", str(min_count))
        .replace("{min_words}", str(min_words))
        .replace("{max_words}", str(max_words))
    )

    client = get_openai_client(llm_config)
    model = get_llm_setting(llm_config, "openai_model")
    now_req = datetime.now().isoformat(timespec="seconds")
    logger(
        filename,
        {
            "type": "suggestions_request",
            "time": now_req,
            "model": model,
            "content": user_prompt,
        },
    )

    last_err = None
    answer = ""
    for attempt in range(3):
        try:
            chat = client.chat.completions.create(
                model=model,
                messages=[{"role": "user", "content": user_prompt}],
            )
            answer = (chat.choices[0].message.content or "").strip()
            break
        except Exception as e:
            last_err = e
            msg = str(e)
            if "429" in msg or "rate" in msg.lower():
                time.sleep(1.5 * (attempt + 1))
                continue
            break
    now = datetime.now().isoformat(timespec="seconds")
    if not answer:
        logger(
            filename,
            {
                "type": "error",
                "time": now,
                "content": str(last_err) if last_err else "empty_suggestions",
            },
        )
        return []
    logger(
        filename,
        {"type": "suggestions_response", "time": now, "content": answer},
    )

    parsed = None
    try:
        parsed = json.loads(answer)
    except Exception:
        try:
            cleaned = answer.strip()
            if cleaned.startswith("```"):
                cleaned = "\n".join(
                    [
                        line
                        for line in cleaned.splitlines()
                        if not line.strip().startswith("```")
                    ]
                )
            parsed = json.loads(cleaned)
        except Exception:
            try:
                m = re.search(r"\[[\s\S]*\]", answer)
                if m:
                    parsed = json.loads(m.group(0))
            except Exception:
                parsed = None
    items = []
    if isinstance(parsed, list):
        for it in parsed:
            text = (it or {}).get("text")
            start = (it or {}).get("start")
            end = (it or {}).get("end")
            if text and start and end:
                items.append(
                    {"text": str(text), "start": str(start), "end": str(end)}
                )
    return items

