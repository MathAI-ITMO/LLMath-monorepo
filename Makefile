BACKEND_PRESENTATION = services/backend/src/MathLLMBackend.Presentation

.PHONY: generate-api export-openapi export-video-openapi codegen

generate-api: export-openapi export-video-openapi codegen

export-openapi:
	cd $(BACKEND_PRESENTATION) && ASPNETCORE_ENVIRONMENT=Development OPENAPI_GENERATION=true dotnet build -c Debug

export-video-openapi:
	cd services/video && python -c "\
import json, sys, os; \
from app import create_app; \
print(json.dumps(create_app().openapi(), indent=2))" > openapi.json

codegen:
	cd services/webui && npm run generate:api
