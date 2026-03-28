from __future__ import annotations

from datetime import datetime

from fastapi import APIRouter, Depends, HTTPException

from ..auth import require_auth
from ..llm import build_timecoded_transcript, generate_suggestions_with_llm
from ..storage import (
    LogStore,
    SubtitleStore,
    SuggestionStore,
    SummaryStore,
)


def create_router(
    summary_store: SummaryStore,
    suggestion_store: SuggestionStore,
    subtitle_store: SubtitleStore,
    log_store: LogStore,
    llm_config: dict,
    config: dict,
) -> APIRouter:
    router = APIRouter()

    @router.get("/summary/{filename:path}")
    def get_summary(filename: str, _: dict = Depends(require_auth)):
        text = summary_store.read(filename)
        return {"text": text}

    @router.get("/suggestions/{filename:path}")
    def get_suggestions(filename: str, _: dict = Depends(require_auth)):
        try:
            existing = suggestion_store.read(filename) or {}
        except Exception as exc:
            return {"items": [], "error": str(exc)}
        items = existing.get("items")
        if isinstance(items, list) and items:
            return {"items": items}
        try:
            segments = subtitle_store.read_segments(filename)
            if segments:
                timecoded = build_timecoded_transcript(segments)
                new_items = generate_suggestions_with_llm(
                    timecoded,
                    filename,
                    subs_count=len(segments),
                    llm_config=llm_config,
                    config=config,
                    logger=log_store.append,
                )
                if new_items:
                    suggestion_store.write_items(filename, new_items)
                    return {"items": new_items}
        except Exception as e:
            log_store.append(
                filename,
                {
                    "type": "error",
                    "time": datetime.now().isoformat(timespec="seconds"),
                    "content": f"suggestions_on_demand_error: {str(e)}",
                },
            )
        return {"items": []}

    @router.get("/logs/{filename:path}")
    def get_logs(filename: str, _: dict = Depends(require_auth)):
        entries = log_store.read_entries(filename)
        return {"entries": entries}

    @router.delete("/logs/{filename:path}")
    def clear_logs(filename: str, _: dict = Depends(require_auth)):
        try:
            log_store.clear(filename)
            return {"status": "cleared"}
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))

    return router
