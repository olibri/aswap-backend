# ---------- runtime layer (не чіпаємо) ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080 8081


# ---------- build layer ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копіюємо csproj-файли
COPY ["Aswap-back/Aswap-back.csproj", "Aswap-back/"]
COPY ["App/App.csproj",               "App/"]
COPY ["Domain/Domain.csproj",         "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# === ДІАГНОСТИКА №1 ===
RUN echo "=== CSPROJ visible after first COPY ===" && \
    find /src -maxdepth 2 -name "*.csproj"

RUN dotnet restore "./Aswap-back/Aswap-back.csproj"

# Копіюємо решту коду
COPY . .

# === ДІАГНОСТИКА №2 ===
RUN echo "=== LS /src (top 50 lines) ===" && \
    ls -R | head -n 50

WORKDIR /src/Aswap-back

# === ДІАГНОСТИКА №3 ===
RUN echo "=== Present in /src/Aswap-back ===" && \
    ls -la

RUN dotnet build "./Aswap-back.csproj" -c $BUILD_CONFIGURATION -o /app/build


# ---------- publish layer ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aswap-back.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# ---------- final runtime image ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aswap-back.dll"]
