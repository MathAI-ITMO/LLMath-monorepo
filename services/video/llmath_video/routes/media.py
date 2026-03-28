from __future__ import annotations

import os

from fastapi import APIRouter, Depends, HTTPException, Query, Request, UploadFile
from fastapi.responses import FileResponse, JSONResponse

from ..auth import require_auth
from ..processing import ProcessingService
from ..storage import FrameStore, SubtitleStore, VideoStore


def create_router(
    video_store: VideoStore,
    subtitle_store: SubtitleStore,
    frame_store: FrameStore,
    dirs: dict,
    processing_service: ProcessingService,
) -> APIRouter:
    router = APIRouter()

    @router.get("/videos")
    def list_videos(_: dict = Depends(require_auth)):
        records = video_store.list_videos()
        return [r.__dict__ for r in records]

    @router.get("/video/{filename:path}")
    def serve_video(filename: str, _: dict = Depends(require_auth)):
        file_path = os.path.join(dirs["video"], os.path.basename(filename))
        if not os.path.isfile(file_path):
            raise HTTPException(status_code=404, detail="Not found")
        return FileResponse(file_path)

    @router.delete("/video/{filename:path}")
    def delete_video(filename: str, _: dict = Depends(require_auth)):
        filename = os.path.basename(filename)
        video_path = os.path.join(dirs["video"], filename)
        base, _ = os.path.splitext(filename)
        audio_path = os.path.join(dirs["audio"], f"{base}.mp3")
        subs_path = subtitle_store.path_for(filename)
        sugg_path = os.path.join(dirs["suggestions"], f"{filename}.json")
        errors = []
        deleted = []
        for path in (video_path, audio_path, subs_path, sugg_path):
            try:
                if os.path.exists(path):
                    os.remove(path)
                    deleted.append(os.path.basename(path))
            except Exception as e:
                errors.append(str(e))
        status = 200 if not errors else 207
        return JSONResponse({"deleted": deleted, "errors": errors}, status_code=status)

    @router.post("/upload", status_code=201)
    async def upload_video(file: UploadFile, _: dict = Depends(require_auth)):
        if not file.filename:
            raise HTTPException(status_code=400, detail="No file selected")
        try:
            name = video_store.save(file.filename, file.file)
        except ValueError as e:
            raise HTTPException(status_code=400, detail=str(e))
        save_path = os.path.join(dirs["video"], name)
        processing_service.queue(save_path)
        return {"name": name, "url": f"/video/{name}"}

    @router.post("/api/ensure_processed")
    async def api_ensure_processed(
        request: Request,
        name: str = Query(default=""),
        _: dict = Depends(require_auth),
    ):
        if not name:
            try:
                data = await request.json()
                name = (data or {}).get("name", "") if isinstance(data, dict) else ""
            except Exception:
                pass
        if not name:
            form = await request.form()
            name = form.get("name", "")
        name = os.path.basename(name)
        if not name:
            raise HTTPException(status_code=400, detail="missing name")
        video_path = video_store.path_for(name)
        if not os.path.isfile(video_path):
            raise HTTPException(status_code=404, detail="not found")
        processing_service.queue(video_path)
        return {"status": "queued"}

    @router.get("/subtitles/{filename:path}.json")
    def serve_subtitles(filename: str, _: dict = Depends(require_auth)):
        segments = subtitle_store.read_segments(filename)
        return {"segments": segments}

    @router.get("/frames/{filename:path}")
    def serve_frame(filename: str, _: dict = Depends(require_auth)):
        try:
            safe_path = frame_store.resolve(filename)
        except ValueError:
            raise HTTPException(status_code=400, detail="invalid path")
        if not os.path.isfile(safe_path):
            raise HTTPException(status_code=404, detail="not found")
        return FileResponse(safe_path)

    @router.get("/favicon.ico", status_code=204)
    def favicon():
        return None

    return router
