# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/InvestmentSimulation.API/InvestmentSimulation.API.csproj", "src/InvestmentSimulation.API/"]
COPY ["src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj", "src/InvestmentSimulation.Application/"]
COPY ["src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj", "src/InvestmentSimulation.Infrastructure/"]
COPY ["src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj", "src/InvestmentSimulation.Domain/"]

RUN dotnet restore "src/InvestmentSimulation.API/InvestmentSimulation.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/InvestmentSimulation.API"
RUN dotnet build "InvestmentSimulation.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "InvestmentSimulation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user for security
RUN addgroup --system --gid 1000 dotnet && \
    adduser --system --uid 1000 --ingroup dotnet --shell /bin/sh dotnet

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R dotnet:dotnet /app

# Switch to non-root user
USER dotnet

ENTRYPOINT ["dotnet", "InvestmentSimulation.API.dll"]
