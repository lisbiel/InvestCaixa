# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas csproj para otimizar cache de restore
COPY ["src/InvestCaixa.API/InvestCaixa.API.csproj", "src/InvestCaixa.API/"]
COPY ["src/InvestCaixa.Application/InvestCaixa.Application.csproj", "src/InvestCaixa.Application/"]
COPY ["src/InvestCaixa.Infrastructure/InvestCaixa.Infrastructure.csproj", "src/InvestCaixa.Infrastructure/"]
COPY ["src/InvestCaixa.Domain/InvestCaixa.Domain.csproj", "src/InvestCaixa.Domain/"]

RUN dotnet restore "src/InvestCaixa.API/InvestCaixa.API.csproj"

# Copia o restante do código
COPY . .

# Build
WORKDIR "/src/src/InvestCaixa.API"
RUN dotnet build "InvestCaixa.API.csproj" -c Release -o /app/build

# =========================
# PUBLISH STAGE
# =========================
FROM build AS publish
WORKDIR "/src/src/InvestCaixa.API"
RUN dotnet publish "InvestCaixa.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

# Usuário não-root para segurança
RUN addgroup --system --gid 1000 dotnet && \
    adduser --system --uid 1000 --ingroup dotnet --shell /bin/sh dotnet

# Cria diretório para dados (SQLite)
RUN mkdir -p /app/data && chown -R dotnet:dotnet /app/data

# Copia artefatos publicados
COPY --from=publish /app/publish .

# Ajusta permissões
RUN chown -R dotnet:dotnet /app

# Switch para usuário não-root
USER dotnet

ENTRYPOINT ["dotnet", "InvestCaixa.API.dll"]
