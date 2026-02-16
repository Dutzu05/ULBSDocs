# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for better layer caching
COPY UlbsDocAuth.Api/UlbsDocAuth.Api.csproj UlbsDocAuth.Api/
RUN dotnet restore UlbsDocAuth.Api/UlbsDocAuth.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish UlbsDocAuth.Api/UlbsDocAuth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

# DOCX->PDF conversion on Linux uses LibreOffice (soffice)
# Spire.Doc can require fonts; install a minimal font set too.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libreoffice \
        fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UlbsDocAuth.Api.dll"]
