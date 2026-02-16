# syntax=docker/dockerfile:1

FROM nginx:1.27-alpine

# Static site
COPY frontend/ /usr/share/nginx/html/

# Reverse proxy to the API service (docker compose service name: api)
COPY frontend/nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
