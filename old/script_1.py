
import os

# Fase Final: Criar TODOS os arquivos que faltam
# Corrigir o problema com arquivos na raiz

missing_files = {
    # Setup.ps1 completo e melhorado
    "setup.ps1": '''# ============================================================================
# Investment Simulation API - Setup Script (PowerShell)
# .NET 8 Clean Architecture
# ============================================================================

Write-Host "╔════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          Investment Simulation API - Configuração Automatizada             ║" -ForegroundColor Cyan
Write-Host "║                    .NET 8 Clean Architecture                              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Verificar se .NET 8 SDK está instalado
Write-Host "🔍 Verificando .NET 8 SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "❌ .NET 8 SDK não encontrado!" -ForegroundColor Red
    Write-Host "   Por favor, instale o .NET 8 SDK: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK identificado: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Criar estrutura de pastas
Write-Host "📁 Criando estrutura de pastas..." -ForegroundColor Yellow
$folders = @(
    "src/InvestmentSimulation.Domain/Entities",
    "src/InvestmentSimulation.Domain/Enums",
    "src/InvestmentSimulation.Domain/Exceptions",
    "src/InvestmentSimulation.Domain/Interfaces",
    "src/InvestmentSimulation.Application/DTOs/Request",
    "src/InvestmentSimulation.Application/DTOs/Response",
    "src/InvestmentSimulation.Application/Interfaces",
    "src/InvestmentSimulation.Application/Services",
    "src/InvestmentSimulation.Application/Validators",
    "src/InvestmentSimulation.Application/Mappings",
    "src/InvestmentSimulation.Application/Common",
    "src/InvestmentSimulation.Infrastructure/Data",
    "src/InvestmentSimulation.Infrastructure/Configurations",
    "src/InvestmentSimulation.Infrastructure/Repositories",
    "src/InvestmentSimulation.Infrastructure/Services",
    "src/InvestmentSimulation.Infrastructure/Migrations",
    "src/InvestmentSimulation.API/Controllers",
    "src/InvestmentSimulation.API/Middlewares",
    "src/InvestmentSimulation.API/Extensions",
    "src/InvestmentSimulation.API/Properties",
    "tests/InvestmentSimulation.UnitTests/Services",
    "tests/InvestmentSimulation.UnitTests/Validators",
    "tests/InvestmentSimulation.UnitTests/Repositories",
    "tests/InvestmentSimulation.UnitTests/IntegrationTests",
    "tests/InvestmentSimulation.UnitTests/Fixtures",
    "tests/InvestmentSimulation.UnitTests/Helpers",
    "Logs"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
}
Write-Host "✅ Pastas criadas" -ForegroundColor Green
Write-Host ""

# Criar solução
Write-Host "📦 Criando solução e projetos..." -ForegroundColor Yellow
if (-not (Test-Path "InvestmentSimulation.sln")) {
    dotnet new sln -n InvestmentSimulation | Out-Null
}

# Criar projetos
$projects = @(
    @{ Type = "classlib"; Name = "InvestmentSimulation.Domain"; Path = "src/InvestmentSimulation.Domain" },
    @{ Type = "classlib"; Name = "InvestmentSimulation.Application"; Path = "src/InvestmentSimulation.Application" },
    @{ Type = "classlib"; Name = "InvestmentSimulation.Infrastructure"; Path = "src/InvestmentSimulation.Infrastructure" },
    @{ Type = "webapi"; Name = "InvestmentSimulation.API"; Path = "src/InvestmentSimulation.API" },
    @{ Type = "xunit"; Name = "InvestmentSimulation.UnitTests"; Path = "tests/InvestmentSimulation.UnitTests" }
)

foreach ($project in $projects) {
    $projectPath = "$($project.Path)/$($project.Name).csproj"
    if (-not (Test-Path $projectPath)) {
        dotnet new $project.Type -n $project.Name -o $project.Path -f | Out-Null
    }
}
Write-Host "✅ Projetos criados" -ForegroundColor Green
Write-Host ""

# Adicionar projetos à solução
Write-Host "🔗 Adicionando projetos à solução..." -ForegroundColor Yellow
@(
    "src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj",
    "src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj",
    "src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj",
    "src/InvestmentSimulation.API/InvestmentSimulation.API.csproj",
    "tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj"
) | ForEach-Object { 
    if (Test-Path $_) {
        dotnet sln add $_ 2>$null
    }
}
Write-Host "✅ Projetos adicionados" -ForegroundColor Green
Write-Host ""

# Configurar referências entre projetos
Write-Host "🌐 Configurando referências..." -ForegroundColor Yellow

# Application → Domain
dotnet add src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj reference src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj 2>$null

# Infrastructure → Application + Domain
dotnet add src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj 2>$null
dotnet add src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj reference src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj 2>$null

# API → Application + Infrastructure
dotnet add src/InvestmentSimulation.API/InvestmentSimulation.API.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj 2>$null
dotnet add src/InvestmentSimulation.API/InvestmentSimulation.API.csproj reference src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj 2>$null

# Tests → Application + Infrastructure
dotnet add tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj reference src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj 2>$null
dotnet add tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj reference src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj 2>$null

Write-Host "✅ Referências configuradas" -ForegroundColor Green
Write-Host ""

# Instalar pacotes NuGet
Write-Host "📦 Instalando pacotes NuGet..." -ForegroundColor Yellow

Set-Location src/InvestmentSimulation.Application
dotnet add package AutoMapper 2>$null
dotnet add package FluentValidation 2>$null
dotnet add package FluentValidation.DependencyInjectionExtensions 2>$null
Set-Location ../..

Set-Location src/InvestmentSimulation.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore 2>$null
dotnet add package Microsoft.EntityFrameworkCore.SqlServer 2>$null
dotnet add package Microsoft.EntityFrameworkCore.Tools 2>$null
dotnet add package Microsoft.EntityFrameworkCore.Design 2>$null
Set-Location ../..

Set-Location src/InvestmentSimulation.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer 2>$null
dotnet add package Serilog.AspNetCore 2>$null
dotnet add package Serilog.Sinks.Console 2>$null
dotnet add package Serilog.Sinks.File 2>$null
dotnet add package Serilog.Sinks.Seq 2>$null
dotnet add package Swashbuckle.AspNetCore 2>$null
Set-Location ../..

Set-Location tests/InvestmentSimulation.UnitTests
dotnet add package xunit 2>$null
dotnet add package xunit.runner.visualstudio 2>$null
dotnet add package Moq 2>$null
dotnet add package FluentAssertions 2>$null
dotnet add package Microsoft.EntityFrameworkCore.InMemory 2>$null
dotnet add package Microsoft.AspNetCore.Mvc.Testing 2>$null
Set-Location ../..

Write-Host "✅ Pacotes instalados" -ForegroundColor Green
Write-Host ""

# Restaurar dependências
Write-Host "🔄 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore 2>$null
Write-Host "✅ Dependências restauradas" -ForegroundColor Green
Write-Host ""

# Mensagem de sucesso
Write-Host "╔════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                     ✅ SETUP CONCLUÍDO COM SUCESSO!                       ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Copie os arquivos de código para os diretórios" -ForegroundColor White
Write-Host "2. Configure appsettings.json com sua connection string" -ForegroundColor White
Write-Host "3. Execute migrations: dotnet ef database update" -ForegroundColor White
Write-Host "4. Inicie a aplicação: dotnet run" -ForegroundColor White
Write-Host "5. Acesse: https://localhost:7001/swagger" -ForegroundColor White
Write-Host ""
''',

    # GlobalUsings para cada projeto (sem directory name prefix)
    "src/InvestmentSimulation.Domain/GlobalUsings.cs": '''global using InvestmentSimulation.Domain.Entities;
global using InvestmentSimulation.Domain.Enums;
global using InvestmentSimulation.Domain.Exceptions;
global using InvestmentSimulation.Domain.Interfaces;
''',

    "src/InvestmentSimulation.Application/GlobalUsings.cs": '''global using InvestmentSimulation.Application.DTOs.Request;
global using InvestmentSimulation.Application.DTOs.Response;
global using InvestmentSimulation.Application.Interfaces;
global using InvestmentSimulation.Application.Services;
global using AutoMapper;
global using FluentValidation;
''',

    "src/InvestmentSimulation.Infrastructure/GlobalUsings.cs": '''global using InvestmentSimulation.Domain.Entities;
global using InvestmentSimulation.Domain.Enums;
global using InvestmentSimulation.Domain.Interfaces;
global using InvestmentSimulation.Infrastructure.Data;
global using Microsoft.EntityFrameworkCore;
''',

    "src/InvestmentSimulation.API/GlobalUsings.cs": '''global using InvestmentSimulation.API.Controllers;
global using InvestmentSimulation.API.Extensions;
global using InvestmentSimulation.API.Middlewares;
global using InvestmentSimulation.Application.DTOs.Request;
global using InvestmentSimulation.Application.DTOs.Response;
global using InvestmentSimulation.Application.Interfaces;
global using InvestmentSimulation.Domain.Exceptions;
global using Microsoft.AspNetCore.Authorization;
''',

    "src/InvestmentSimulation.API/Properties/launchSettings.json": '''{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
''',

    ".editorconfig": '''root = true

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8
end_of_line = crlf
trim_trailing_whitespace = true
insert_final_newline = true

# C# Coding Conventions
csharp_style_namespace_declarations = file_scoped:silent
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true

# Naming conventions
dotnet_naming_rule.interface_should_be_begins_with_i.severity = suggestion
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case
''',
}

# Criar arquivos filtrando os que estão em subdirectórios
for path, content in missing_files.items():
    dir_path = os.path.dirname(path)
    if dir_path:  # Se tem diretório
        os.makedirs(dir_path, exist_ok=True)
    
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

print("✅ Arquivos faltantes criados com sucesso!")
print(f"📦 Total de arquivos criados: {len(missing_files)}")
print("\nArquivos criados:")
for path in sorted(missing_files.keys()):
    print(f"  ✓ {path}")
