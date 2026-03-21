FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

COPY *.sln .
COPY Directory.Packages.props .

RUN mkdir -p src/MathLLMBackend.Presentation \
    src/MathLLMBackend.Core \
    src/MathLLMBackend.DataAccess \
    src/MathLLMBackend.Domain \
    src/MathLLMBackend.GeolinClient 

COPY src/MathLLMBackend.Presentation/MathLLMBackend.Presentation.csproj ./src/MathLLMBackend.Presentation/
COPY src/MathLLMBackend.Core/MathLLMBackend.Core.csproj ./src/MathLLMBackend.Core/
COPY src/MathLLMBackend.DataAccess/MathLLMBackend.DataAccess.csproj ./src/MathLLMBackend.DataAccess/
COPY src/MathLLMBackend.Domain/MathLLMBackend.Domain.csproj ./src/MathLLMBackend.Domain/
COPY src/MathLLMBackend.GeolinClient/MathLLMBackend.GeolinClient.csproj ./src/MathLLMBackend.GeolinClient/

RUN dotnet restore src/MathLLMBackend.Presentation/MathLLMBackend.Presentation.csproj

COPY . .

RUN dotnet build src/MathLLMBackend.Presentation/MathLLMBackend.Presentation.csproj -c Release -o /App/out /p:OpenApiGenerateDocumentsOnBuild=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /App

RUN useradd -m -s /bin/bash appuser && chown -R appuser /App
USER appuser

COPY --from=build --chown=appuser:appuser /App/out .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "MathLLMBackend.Presentation.dll"]