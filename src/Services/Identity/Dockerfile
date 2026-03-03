# ── Build stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and props files for nuget restore caching
COPY Directory.Build.props Directory.Packages.props global.json StayHub.slnx ./

# Copy all project files (fast layer caching)
COPY src/Shared/StayHub.Shared/StayHub.Shared.csproj src/Shared/StayHub.Shared/
COPY src/Shared/StayHub.Shared.Infrastructure/StayHub.Shared.Infrastructure.csproj src/Shared/StayHub.Shared.Infrastructure/
COPY src/Shared/StayHub.Shared.Web/StayHub.Shared.Web.csproj src/Shared/StayHub.Shared.Web/
COPY src/Services/Identity/StayHub.Services.Identity.Domain/StayHub.Services.Identity.Domain.csproj src/Services/Identity/StayHub.Services.Identity.Domain/
COPY src/Services/Identity/StayHub.Services.Identity.Application/StayHub.Services.Identity.Application.csproj src/Services/Identity/StayHub.Services.Identity.Application/
COPY src/Services/Identity/StayHub.Services.Identity.Infrastructure/StayHub.Services.Identity.Infrastructure.csproj src/Services/Identity/StayHub.Services.Identity.Infrastructure/
COPY src/Services/Identity/StayHub.Services.Identity.Api/StayHub.Services.Identity.Api.csproj src/Services/Identity/StayHub.Services.Identity.Api/

# Restore dependencies
RUN dotnet restore src/Services/Identity/StayHub.Services.Identity.Api/StayHub.Services.Identity.Api.csproj

# Copy everything
COPY src/ src/

# Build and publish
WORKDIR /src/src/Services/Identity/StayHub.Services.Identity.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Security: run as non-root user
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayHub.Services.Identity.Api.dll"]
