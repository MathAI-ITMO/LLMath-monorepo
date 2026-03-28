from __future__ import annotations

import os

import requests
from fastapi import HTTPException, Request


def _get_backend_url(request: Request) -> str:
    env = os.environ.get("VIDEOAPP_BACKEND_URL")
    if env:
        return env.rstrip("/")
    settings = request.app.state.settings
    return settings.get("config", {}).get("backend_url", "http://localhost:5000")


async def require_auth(request: Request) -> dict:
    cookie_header = request.headers.get("Cookie", "")
    if not cookie_header:
        raise HTTPException(status_code=401, detail="Missing authentication")
    backend_url = _get_backend_url(request)
    try:
        response = requests.get(
            f"{backend_url}/api/User/me",
            headers={"Cookie": cookie_header},
            timeout=5,
        )
    except Exception:
        raise HTTPException(status_code=503, detail="Auth service unavailable")
    if response.status_code != 200:
        raise HTTPException(status_code=401, detail="Invalid or expired session")
    return response.json()
