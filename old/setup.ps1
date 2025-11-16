param(
    [string]$ProjectName = "InvestmentSimulation"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup .NET 8 - $ProjectName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar .NET
Write-Host "Verificando .NET..." -ForegroundColor Yellow
dotnet --version
Write-Host ""

# Criar solução
Write-Host "Criando solução..." -ForegroundColor Yellow
dotnet new sln -n $ProjectName -o . --force
Write-Host "OK" -ForegroundColor Green
Write-Host ""

# Criar Domain
Write-Host "Criando Domain Layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ProjectName.Domain" -o "src\$ProjectName.Domain" --force
Write-Host "OK" -ForegroundColor Green

# Criar Application
Write-Host "Criando Application Layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ProjectName.Application" -o "src\$ProjectName.Application" --force
Write-Host "OK" -ForegroundColor Green

# Criar Infrastructure
Write-Host "Criando Infrastructure Layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ProjectName.Infrastructure" -o "src\$ProjectName.Infrastructure" --force
Write-Host "OK" -ForegroundColor Green

# Criar API
Write-Host "Criando API Layer..." -ForegroundColor Yellow
dotnet new webapi -n "$ProjectName.API" -o "src\$ProjectName.API" --force
Write-Host "OK" -ForegroundColor Green

# Criar Tests
Write-Host "Criando Test Layer..." -ForegroundColor Yellow
dotnet new xunit -n "$ProjectName.UnitTests" -o "tests\$ProjectName.UnitTests" --force
Write-Host "OK" -ForegroundColor Green
Write-Host ""

# Adicionar à solução
Write-Host "Adicionando à solução..." -ForegroundColor Yellow
dotnet sln add "src\$ProjectName.Domain\$ProjectName.Domain.csproj"
dotnet sln add "src\$ProjectName.Application\$ProjectName.Application.csproj"
dotnet sln add "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj"
dotnet sln add "src\$ProjectName.API\$ProjectName.API.csproj"
dotnet sln add "tests\$ProjectName.UnitTests\$ProjectName.UnitTests.csproj"
Write-Host "OK" -ForegroundColor Green
Write-Host ""

# Adicionar referências
Write-Host "Adicionando referências..." -ForegroundColor Yellow
dotnet add "src\$ProjectName.Application\$ProjectName.Application.csproj" reference "src\$ProjectName.Domain\$ProjectName.Domain.csproj"
dotnet add "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj" reference "src\$ProjectName.Domain\$ProjectName.Domain.csproj"
dotnet add "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj" reference "src\$ProjectName.Application\$ProjectName.Application.csproj"
dotnet add "src\$ProjectName.API\$ProjectName.API.csproj" reference "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj"
dotnet add "src\$ProjectName.API\$ProjectName.API.csproj" reference "src\$ProjectName.Application\$ProjectName.Application.csproj"
dotnet add "tests\$ProjectName.UnitTests\$ProjectName.UnitTests.csproj" reference "src\$ProjectName.Application\$ProjectName.Application.csproj"
dotnet add "tests\$ProjectName.UnitTests\$ProjectName.UnitTests.csproj" reference "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj"
Write-Host "OK" -ForegroundColor Green
Write-Host ""

# Instalar pacotes
Write-Host "Instalando pacotes NuGet..." -ForegroundColor Yellow

Write-Host "  - Application..." -ForegroundColor Gray
Push-Location "src\$ProjectName.Application"
dotnet add package AutoMapper
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
Pop-Location

Write-Host "  - Infrastructure..." -ForegroundColor Gray
Push-Location "src\$ProjectName.Infrastructure"
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
Pop-Location

Write-Host "  - API..." -ForegroundColor Gray
Push-Location "src\$ProjectName.API"
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Swashbuckle.AspNetCore
Pop-Location

Write-Host "  - Tests..." -ForegroundColor Gray
Push-Location "tests\$ProjectName.UnitTests"
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.AspNetCore.Mvc.Testing
Pop-Location

Write-Host "OK" -ForegroundColor Green
Write-Host ""

# Restaurar
Write-Host "Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore
Write-Host "OK" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "SETUP CONCLUIDO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Projeto: $ProjectName" -ForegroundColor Green
Write-Host ""
Write-Host "Proximos passos:" -ForegroundColor Cyan
Write-Host "1. code .                          (abrir VS Code)"
Write-Host "2. Editar appsettings.json"
Write-Host "3. cd src\$ProjectName.API"
Write-Host "4. dotnet ef migrations add InitialCreate --project ..\$ProjectName.Infrastructure"
Write-Host "5. dotnet ef database update --project ..\$ProjectName.Infrastructure"
Write-Host "6. dotnet run"
Write-Host "7. https://localhost:7001/swagger"
Write-Host ""
