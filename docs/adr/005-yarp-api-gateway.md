# ADR-005: YARP as API Gateway

**Date**: 2026-03-02
**Status**: Accepted

## Context

With 7 microservices, the frontend needs a single entry point. An API gateway handles routing, authentication forwarding, rate limiting, and CORS centrally instead of duplicating it across services.

## Options Considered

1. **YARP (Yet Another Reverse Proxy)** — Microsoft-built reverse proxy on ASP.NET Core
2. **Ocelot** — Popular .NET API gateway
3. **Kong** — Cloud-native API gateway (Go/Lua)
4. **Nginx** — General-purpose reverse proxy

## Decision

Use **YARP** as the API Gateway.

## Rationale

- **Microsoft-built**: Actively maintained by the .NET team, production-proven at Microsoft scale
- **ASP.NET Core native**: Runs on the same pipeline we know — can use standard middleware for auth, logging, rate limiting
- **Performance**: Built on top of Kestrel, one of the fastest web servers
- **Configuration**: Routes can be configured via `appsettings.json` or dynamically via code
- **No separate tech stack**: Unlike Kong (Lua) or Nginx (config language), YARP is pure C#

## Consequences

### Positive
- Single entry point for all API traffic
- Centralized rate limiting, CORS, and request logging
- Auth token forwarded to downstream services automatically
- Health check aggregation across all services
- Same debugging and profiling tools as our services

### Negative
- Single point of failure (mitigated by running multiple replicas in K8s)
- Additional network hop for every request
- Must keep route configuration in sync with service changes
