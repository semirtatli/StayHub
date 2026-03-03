# ── Build stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json StayHub.slnx ./

COPY src/Shared/StayHub.Shared/StayHub.Shared.csproj src/Shared/StayHub.Shared/
COPY src/Shared/StayHub.Shared.Infrastructure/StayHub.Shared.Infrastructure.csproj src/Shared/StayHub.Shared.Infrastructure/
COPY src/Shared/StayHub.Shared.Web/StayHub.Shared.Web.csproj src/Shared/StayHub.Shared.Web/
COPY src/Services/Hotel/StayHub.Services.Hotel.Domain/StayHub.Services.Hotel.Domain.csproj src/Services/Hotel/StayHub.Services.Hotel.Domain/
COPY src/Services/Hotel/StayHub.Services.Hotel.Application/StayHub.Services.Hotel.Application.csproj src/Services/Hotel/StayHub.Services.Hotel.Application/
COPY src/Services/Hotel/StayHub.Services.Hotel.Infrastructure/StayHub.Services.Hotel.Infrastructure.csproj src/Services/Hotel/StayHub.Services.Hotel.Infrastructure/
COPY src/Services/Hotel/StayHub.Services.Hotel.Api/StayHub.Services.Hotel.Api.csproj src/Services/Hotel/StayHub.Services.Hotel.Api/

RUN dotnet restore src/Services/Hotel/StayHub.Services.Hotel.Api/StayHub.Services.Hotel.Api.csproj

COPY src/ src/

WORKDIR /src/src/Services/Hotel/StayHub.Services.Hotel.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayHub.Services.Hotel.Api.dll"]
