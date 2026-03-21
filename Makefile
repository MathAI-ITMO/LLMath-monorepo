BACKEND_PRESENTATION = services/backend/src/MathLLMBackend.Presentation

.PHONY: generate-api export-openapi codegen

generate-api: export-openapi codegen

export-openapi:
	cd $(BACKEND_PRESENTATION) && ASPNETCORE_ENVIRONMENT=Development OPENAPI_GENERATION=true dotnet build -c Debug

codegen:
	cd services/webui && npm run generate:api
