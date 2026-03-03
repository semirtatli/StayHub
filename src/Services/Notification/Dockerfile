# ── Build stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json StayHub.slnx ./

COPY src/Shared/StayHub.Shared/StayHub.Shared.csproj src/Shared/StayHub.Shared/
COPY src/Shared/StayHub.Shared.Infrastructure/StayHub.Shared.Infrastructure.csproj src/Shared/StayHub.Shared.Infrastructure/
COPY src/Shared/StayHub.Shared.Web/StayHub.Shared.Web.csproj src/Shared/StayHub.Shared.Web/
COPY src/Services/Notification/StayHub.Services.Notification.Domain/StayHub.Services.Notification.Domain.csproj src/Services/Notification/StayHub.Services.Notification.Domain/
COPY src/Services/Notification/StayHub.Services.Notification.Application/StayHub.Services.Notification.Application.csproj src/Services/Notification/StayHub.Services.Notification.Application/
COPY src/Services/Notification/StayHub.Services.Notification.Infrastructure/StayHub.Services.Notification.Infrastructure.csproj src/Services/Notification/StayHub.Services.Notification.Infrastructure/
COPY src/Services/Notification/StayHub.Services.Notification.Api/StayHub.Services.Notification.Api.csproj src/Services/Notification/StayHub.Services.Notification.Api/

RUN dotnet restore src/Services/Notification/StayHub.Services.Notification.Api/StayHub.Services.Notification.Api.csproj

COPY src/ src/

WORKDIR /src/src/Services/Notification/StayHub.Services.Notification.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser

COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayHub.Services.Notification.Api.dll"]
