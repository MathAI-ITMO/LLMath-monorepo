# syntax=docker/dockerfile:1

FROM python:3.11-slim AS base

ARG INCLUDE_WHISPER=true

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg && \
    rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --upgrade pip && \
    if [ "$INCLUDE_WHISPER" = "true" ]; then \
        pip install -r requirements.txt; \
    else \
        grep -v "openai-whisper" requirements.txt | pip install -r /dev/stdin; \
    fi

COPY app.py config_manager.py config.json ./
COPY llmath_video/ ./llmath_video/
COPY templates/ ./templates/

ENV PORT=5001
EXPOSE 5001

CMD ["uvicorn", "app:application", "--host", "0.0.0.0", "--port", "5001", "--timeout-keep-alive", "300"]
