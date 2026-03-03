# ── Build stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json StayHub.slnx ./

COPY src/Shared/StayHub.Shared/StayHub.Shared.csproj src/Shared/StayHub.Shared/
COPY src/Shared/StayHub.Shared.Infrastructure/StayHub.Shared.Infrastructure.csproj src/Shared/StayHub.Shared.Infrastructure/
COPY src/Shared/StayHub.Shared.Web/StayHub.Shared.Web.csproj src/Shared/StayHub.Shared.Web/
COPY src/Services/Payment/StayHub.Services.Payment.Domain/StayHub.Services.Payment.Domain.csproj src/Services/Payment/StayHub.Services.Payment.Domain/
COPY src/Services/Payment/StayHub.Services.Payment.Application/StayHub.Services.Payment.Application.csproj src/Services/Payment/StayHub.Services.Payment.Application/
COPY src/Services/Payment/StayHub.Services.Payment.Infrastructure/StayHub.Services.Payment.Infrastructure.csproj src/Services/Payment/StayHub.Services.Payment.Infrastructure/
COPY src/Services/Payment/StayHub.Services.Payment.Api/StayHub.Services.Payment.Api.csproj src/Services/Payment/StayHub.Services.Payment.Api/

RUN dotnet restore src/Services/Payment/StayHub.Services.Payment.Api/StayHub.Services.Payment.Api.csproj

COPY src/ src/

WORKDIR /src/src/Services/Payment/StayHub.Services.Payment.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser

COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayHub.Services.Payment.Api.dll"]
