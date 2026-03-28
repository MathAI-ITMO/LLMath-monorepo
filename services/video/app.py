import os

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from config_manager import is_cors_disabled, load_config, resolve_cors_origins
from llmath_video import load_settings
from llmath_video.logging_setup import setup_logging
from llmath_video.processing import ProcessingService
from llmath_video.routes import content, llm_routes, media
from llmath_video.storage import (
    FrameStore,
    LogStore,
    SubtitleStore,
    SuggestionStore,
    SummaryStore,
    VideoStore,
)


def create_app() -> FastAPI:
    """Create and configure the FastAPI application."""
    base_dir = os.path.abspath(os.path.dirname(__file__))
    settings = load_settings(base_dir)
    setup_logging(settings.dirs.logs, level="INFO")

    video_store = VideoStore(settings.dirs.video, settings.allowed_extensions)
    subtitle_store = SubtitleStore(settings.dirs.subtitles)
    summary_store = SummaryStore(settings.dirs.summaries)
    suggestion_store = SuggestionStore(settings.dirs.suggestions)
    log_store = LogStore(settings.dirs.logs)
    frame_store = FrameStore(settings.dirs.frames)

    dir_map = {
        "video": settings.dirs.video,
        "audio": settings.dirs.audio,
        "subtitles": settings.dirs.subtitles,
        "frames": settings.dirs.frames,
        "summaries": settings.dirs.summaries,
        "logs": settings.dirs.logs,
        "suggestions": settings.dirs.suggestions,
        "base": settings.base_dir,
    }
    processing_service = ProcessingService(
        settings.llm_config,
        settings.config,
        dir_map,
        log_store,
        summary_store,
    )

    app = FastAPI()
    app.state.settings = settings.as_dict()

    if not is_cors_disabled():
        cors_origins = resolve_cors_origins(settings.config)
        app.add_middleware(
            CORSMiddleware,
            allow_origins=cors_origins,
            allow_credentials=True,
            allow_methods=["*"],
            allow_headers=["*"],
        )

    app.include_router(
        media.create_router(
            video_store, subtitle_store, frame_store, dir_map, processing_service
        )
    )
    app.include_router(
        content.create_router(
            summary_store,
            suggestion_store,
            subtitle_store,
            log_store,
            settings.llm_config,
            settings.config,
        )
    )
    app.include_router(
        llm_routes.create_router(
            frame_store,
            summary_store,
            subtitle_store,
            log_store,
            settings.llm_config,
            settings.config,
        )
    )

    return app


application = create_app()

if __name__ == "__main__":
    import uvicorn

    base_dir = os.path.abspath(os.path.dirname(__file__))
    server_config = load_config(base_dir).get("server", {})
    host = os.environ.get(
        "FLASK_RUN_HOST", server_config.get("host", "0.0.0.0")
    )
    port = int(os.environ.get("FLASK_RUN_PORT", server_config.get("port", 5001)))
    uvicorn.run(application, host=host, port=port)
