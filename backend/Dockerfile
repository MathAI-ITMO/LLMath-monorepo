# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /App

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the code and build
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8 AS final
WORKDIR /App

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /App
USER appuser

# Copy published artifacts from build stage
COPY --from=build --chown=appuser:appuser /App/out .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Add metadata labels

# Expose port
EXPOSE 5000

# Set the entry point
ENTRYPOINT ["dotnet", "MathLLMBackend.Presentation.dll"]