from __future__ import annotations

import os
from functools import wraps

import requests
from flask import current_app, jsonify, request


def get_backend_url():
    env = os.environ.get("VIDEOAPP_BACKEND_URL")
    if env:
        return env.rstrip("/")
    settings = current_app.config.get("APP_SETTINGS", {})
    return settings.get("config", {}).get("backend_url", "http://localhost:5000")


def require_auth(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        cookie_header = request.headers.get("Cookie", "")
        if not cookie_header:
            return jsonify({"error": "Missing authentication"}), 401
        backend_url = get_backend_url()
        try:
            response = requests.get(
                f"{backend_url}/api/User/me",
                headers={"Cookie": cookie_header},
                timeout=5,
            )
        except Exception:
            return jsonify({"error": "Auth service unavailable"}), 503
        if response.status_code != 200:
            return jsonify({"error": "Invalid or expired session"}), 401
        request.user_info = response.json()
        return f(*args, **kwargs)
    return decorated
