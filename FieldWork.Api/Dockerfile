# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first → restore layer caches NuGet downloads.
# Matches the REAL solution: Api / Application / Domain / Infrastructure.
# (Tests deliberately excluded from publish.)
COPY ["FieldWork.Api/FieldWork.Api.csproj", "FieldWork.Api/"]
COPY ["FieldWork.Application/FieldWork.Application.csproj", "FieldWork.Application/"]
COPY ["FieldWork.Domain/FieldWork.Domain.csproj", "FieldWork.Domain/"]
COPY ["FieldWork.Infrastructure/FieldWork.Infrastructure.csproj", "FieldWork.Infrastructure/"]

RUN dotnet restore FieldWork.Api/FieldWork.Api.csproj

COPY . .
RUN dotnet publish FieldWork.Api/FieldWork.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -fsS http://localhost:8080/health/live || exit 1

USER $APP_UID
ENTRYPOINT ["dotnet", "FieldWork.Api.dll"]