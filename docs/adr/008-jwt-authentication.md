# ADR-008: JWT Authentication with Refresh Tokens

**Date**: 2026-03-02
**Status**: Accepted

## Context

With microservices behind an API gateway, we need a stateless authentication mechanism that can be verified by any service without calling the Identity Service on every request.

## Options Considered

1. **Session-based auth** — Server stores session, client sends cookie
2. **JWT (access token only)** — Stateless token with claims, verified by signature
3. **JWT access + refresh tokens** — Short-lived access token + long-lived refresh token for renewal
4. **OAuth2 with external provider** — Azure AD B2C or Duende IdentityServer

## Decision

Use **JWT access tokens + refresh tokens** with ASP.NET Core Identity for user management.

## Rationale

- **Stateless verification**: Any service validates the JWT by checking the signature — no database call needed
- **Short-lived access tokens (15 min)**: If token is compromised, the damage window is small
- **Refresh tokens (7 days, rotating)**: User doesn't re-enter credentials frequently; each use issues a new pair
- **Self-contained**: No external dependency (Azure AD) needed for development or demos
- **Claims-based**: User roles and ID are embedded in the token — services read them without additional lookups

## Token Strategy

| Token | Lifetime | Storage | Purpose |
|-------|----------|---------|---------|
| Access Token | 15 minutes | JavaScript memory (NOT localStorage) | API authorization |
| Refresh Token | 7 days | httpOnly secure cookie | Obtain new access token |

### Security Considerations

- **Access token in memory**: Not vulnerable to XSS (unlike localStorage)
- **Refresh token in httpOnly cookie**: Not accessible via JavaScript
- **Token rotation**: Each refresh token use invalidates the old one — stolen refresh tokens are detected
- **Refresh token stored in DB**: Can be revoked server-side (logout, password change, suspicious activity)

## Consequences

### Positive
- Stateless auth scales horizontally — any service instance validates tokens
- No shared session store needed between services
- Works offline (JWT contains all needed claims)
- Token revocation possible via refresh token invalidation

### Negative
- Access token cannot be revoked before expiry (15 min window)
- JWT payload increases header size on every request
- Shared signing key must be distributed to all services securely
- Token refresh logic adds frontend complexity
