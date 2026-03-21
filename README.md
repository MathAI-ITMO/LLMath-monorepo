# MathLLM
WARNING! README COULD HAVE ERRORS, I'VE GENERATED IT WITH DEEPSEEK

A math learning platform with LLM-assisted problem solving and video content.

## Services

| Service | Stack | Port | Path |
|---|---|---|---|
| **backend** | C# / .NET 10 / ASP.NET Core | 5000 | `services/backend` |
| **webui** | TypeScript / Vue 3 / Vite | 3000 | `services/webui` |
| **video** | Python / Flask | 5001 | `services/video` |

### Backend
REST API handling authentication, chat, LLM problem solving, and user task management. Uses PostgreSQL and proxies geometry problems from the Geolin service.

Swagger UI: `http://localhost:5000/swagger`

See [`services/backend/Readme.md`](services/backend/Readme.md) for full API reference and setup instructions.

### WebUI
SPA providing the user-facing interface — chat, problem solving, statistics, and video integration.

See [`services/webui/README.md`](services/webui/README.md) for setup instructions.

### Video
Video player and processing service with LLM integration and speech-to-text via Whisper/OpenAI.

See [`services/video/README.md`](services/video/README.md) for setup instructions.

## Local Development

### Backend
```sh
# Start PostgreSQL
docker compose -f services/backend/docker-compose.yaml up -d

# Apply migrations
dotnet ef database update \
  --project services/backend/src/MathLLMBackend.DataAccess \
  --startup-project services/backend/src/MathLLMBackend.Presentation

# Run
dotnet run --project services/backend/src/MathLLMBackend.Presentation
```

### WebUI
```sh
cd services/webui
npm install
npm run dev
```

### Video
```sh
cd services/video
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate
pip install -r requirements.txt
python app.py
```

## API Client Generation

The frontend TypeScript API client (`services/webui/src/api/generated/`) is auto-generated from the backend OpenAPI spec using [Orval](https://orval.dev/). After changing backend controllers or DTOs, regenerate it with:

```sh
make generate-api
```

This runs two steps:
1. **`export-openapi`** — builds the backend and exports the OpenAPI spec to `services/backend/MathLLMBackend.Presentation_openapi.json`
2. **`codegen`** — runs Orval to regenerate `services/webui/src/api/generated/api.ts` from that spec

> Do not edit `services/webui/src/api/generated/` manually — changes will be overwritten.

## Environment Variables

Each service has its own `.env` file. Copy the example files to get started:

```sh
cp services/webui/.env.example services/webui/.env
```

For the video service, configure `VIDEOAPP_*` variables (see [`services/video/README.md`](services/video/README.md#31-настройка-переменных-окружения)).
