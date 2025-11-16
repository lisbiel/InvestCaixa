
import os

# Fase Final - Part 2: Criar todos os .csproj files

csproj_files = {
    "src/InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj": '''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

</Project>
''',

    "src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj": '''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="FluentValidation" Version="11.9.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj" />
  </ItemGroup>

</Project>
''',

    "src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj": '''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../InvestmentSimulation.Domain/InvestmentSimulation.Domain.csproj" />
    <ProjectReference Include="../InvestmentSimulation.Application/InvestmentSimulation.Application.csproj" />
  </ItemGroup>

</Project>
''',

    "src/InvestmentSimulation.API/InvestmentSimulation.API.csproj": '''<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="7.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../InvestmentSimulation.Application/InvestmentSimulation.Application.csproj" />
    <ProjectReference Include="../InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj" />
  </ItemGroup>

</Project>
''',

    "tests/InvestmentSimulation.UnitTests/InvestmentSimulation.UnitTests.csproj": '''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.18.4" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/InvestmentSimulation.Application/InvestmentSimulation.Application.csproj" />
    <ProjectReference Include="../../src/InvestmentSimulation.Infrastructure/InvestmentSimulation.Infrastructure.csproj" />
  </ItemGroup>

</Project>
''',
}

for path, content in csproj_files.items():
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

print("✅ Arquivos .csproj criados com sucesso!")
print(f"📦 Total de arquivos .csproj: {len(csproj_files)}")
