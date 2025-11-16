#!/bin/bash

echo "======================================"
echo "Investment Simulation API - Setup"
echo "======================================"

# Verificar se .NET 8 SDK está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK não encontrado. Por favor, instale antes de continuar."
    exit 1
fi

echo "✅ .NET 8 SDK encontrado"

# Criar solução
echo "📦 Criando solução..."
dotnet new sln -n InvestmentSimulation

# Criar projetos
echo "📦 Criando projetos..."
dotnet new classlib -n InvestmentSimulation.Domain -o src/InvestmentSimulation.Domain
dotnet new classlib -n InvestmentSimulation.Application -o src/InvestmentSimulation.Application
dotnet new classlib -n InvestmentSimulation.Infrastructure -o src/InvestmentSimulation.Infrastructure
dotnet new webapi -n InvestmentSimulation.API -o src/InvestmentSimulation.API
dotnet new xunit -n InvestmentSimulation.UnitTests -o tests/InvestmentSimulation.UnitTests

# Adicionar projetos à solução
echo "📦 Adicionando projetos à solução..."
dotnet sln add src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj
dotnet sln add src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj
dotnet sln add src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj
dotnet sln add src/InvestmentSimulation.API/InvestmentSimulation.API.csproj
dotnet sln add tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj

# Adicionar referências
echo "🔗 Configurando referências entre projetos..."
dotnet add src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj reference src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj
dotnet add src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj
dotnet add src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj reference src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj
dotnet add src/InvestmentSimulation.API/InvestmentSimulation.API.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj
dotnet add src/InvestmentSimulation.API/InvestmentSimulation.API.csproj reference src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj
dotnet add tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj
dotnet add tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj reference src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj

# Restaurar dependências
echo "🔄 Restaurando dependências..."
dotnet restore

echo ""
echo "✅ Setup concluído com sucesso!"
echo ""
echo "Próximos passos:"
echo "1. Execute: cd src/InvestmentSimulation.API"
echo "2. Execute: dotnet ef migrations add InitialCreate --project ../InvestmentSimulation.Infrastructure"
echo "3. Execute: dotnet ef database update --project ../InvestmentSimulation.Infrastructure"
echo "4. Execute: dotnet run"
