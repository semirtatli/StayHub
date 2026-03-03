# ── Build stage ───────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

COPY src/Frontend/stayhub-web/package.json src/Frontend/stayhub-web/package-lock.json ./
RUN npm ci

COPY src/Frontend/stayhub-web/ .
RUN npm run build

# ── Runtime stage (Nginx) ────────────────────────────────────────────
FROM nginx:alpine AS runtime

# Copy custom nginx config for SPA routing
COPY deploy/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
