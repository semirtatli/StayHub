# ── Build stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json StayHub.slnx ./

COPY src/Shared/StayHub.Shared/StayHub.Shared.csproj src/Shared/StayHub.Shared/
COPY src/Shared/StayHub.Shared.Infrastructure/StayHub.Shared.Infrastructure.csproj src/Shared/StayHub.Shared.Infrastructure/
COPY src/Shared/StayHub.Shared.Web/StayHub.Shared.Web.csproj src/Shared/StayHub.Shared.Web/
COPY src/Services/Booking/StayHub.Services.Booking.Domain/StayHub.Services.Booking.Domain.csproj src/Services/Booking/StayHub.Services.Booking.Domain/
COPY src/Services/Booking/StayHub.Services.Booking.Application/StayHub.Services.Booking.Application.csproj src/Services/Booking/StayHub.Services.Booking.Application/
COPY src/Services/Booking/StayHub.Services.Booking.Infrastructure/StayHub.Services.Booking.Infrastructure.csproj src/Services/Booking/StayHub.Services.Booking.Infrastructure/
COPY src/Services/Booking/StayHub.Services.Booking.Api/StayHub.Services.Booking.Api.csproj src/Services/Booking/StayHub.Services.Booking.Api/

RUN dotnet restore src/Services/Booking/StayHub.Services.Booking.Api/StayHub.Services.Booking.Api.csproj

COPY src/ src/

WORKDIR /src/src/Services/Booking/StayHub.Services.Booking.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser

COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayHub.Services.Booking.Api.dll"]
