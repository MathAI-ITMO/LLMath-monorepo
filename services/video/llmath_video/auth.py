from __future__ import annotations

from functools import wraps

import requests
from flask import current_app, jsonify, request


def get_backend_url():
    settings = current_app.config.get("APP_SETTINGS", {})
    return settings.get("config", {}).get("backend_url", "http://localhost:5000")


def validate_token(token: str) -> tuple[bool, dict | None]:
    backend_url = get_backend_url()
    validate_url = f"{backend_url}/api/Auth/validate"
    try:
        response = requests.get(
            validate_url,
            headers={"Authorization": f"Bearer {token}"},
            timeout=5,
        )
        if response.status_code == 200:
            return True, response.json()
        return False, None
    except Exception:
        return False, None


def require_auth(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        auth_header = request.headers.get("Authorization", "")
        if not auth_header.startswith("Bearer "):
            return jsonify({"error": "Missing or invalid authorization header"}), 401
        token = auth_header[7:]
        if not token:
            return jsonify({"error": "Missing token"}), 401
        valid, user_info = validate_token(token)
        if not valid:
            return jsonify({"error": "Invalid or expired token"}), 401
        request.user_info = user_info
        return f(*args, **kwargs)
    return decorated