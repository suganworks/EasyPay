# ─── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (layer caching)
COPY ["EasyPay.API/EasyPay.API.csproj",             "EasyPay.API/"]
COPY ["EasyPay.Core/EasyPay.Core.csproj",           "EasyPay.Core/"]
COPY ["EasyPay.Infrastructure/EasyPay.Infrastructure.csproj", "EasyPay.Infrastructure/"]

RUN dotnet restore "EasyPay.API/EasyPay.API.csproj"

# Copy all source
COPY . .

WORKDIR "/src/EasyPay.API"
RUN dotnet build "EasyPay.API.csproj" -c Release -o /app/build

# ─── Publish stage ────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "EasyPay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Create log directory
RUN mkdir -p /app/Logs

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EasyPay.API.dll"]
