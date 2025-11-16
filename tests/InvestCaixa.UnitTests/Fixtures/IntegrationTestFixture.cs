namespace InvestCaixa.UnitTests.Fixtures;

using InvestCaixa.API;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; private set; }

    public IntegrationTestFixture()
    {
        _factory = new ApiWebApplicationFactory();

        Client = _factory.CreateClient();

        var bearerToken = GerarTokenValido();
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
    }

    private string GerarTokenValido()
    {
        using var scope = _factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var jwtSection = config.GetSection("Jwt");
        var secretKey = jwtSection["Secret"]!;
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, "admin"),
        new Claim("name", "admin")
    };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task InitializeAsync()
    {
        // Cria um escopo só para preparar o banco
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvestimentoDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        await SeedDatabaseAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    private static async Task SeedDatabaseAsync(InvestimentoDbContext dbContext)
    {
        var produtos = new List<ProdutoInvestimento>
        {
            new("CDB Caixa 2026", TipoProduto.CDB, 0.12m, NivelRisco.Baixo, 180, 1000m, true, PerfilInvestidor.Conservador),
            new("LCI Plus", TipoProduto.LCI, 0.11m, NivelRisco.Baixo, 90, 5000m, false, PerfilInvestidor.Conservador),
            new("Fundo Multimercado XPTO", TipoProduto.Fundo, 0.18m, NivelRisco.Alto, 0, 500m, true, PerfilInvestidor.Agressivo),
            new("Tesouro IPCA+ 2035", TipoProduto.TesouroDireto, 0.06m, NivelRisco.Baixo, 365, 30m, true, PerfilInvestidor.Conservador),
            new("LCA Desenvolvimento", TipoProduto.LCA, 0.10m, NivelRisco.Baixo, 120, 2000m, false, PerfilInvestidor.Moderado)
        };

        dbContext.Produtos.AddRange(produtos);

        var cliente = new Cliente("João Silva", "joao@test.com", "12345678901", DateTime.Now.AddYears(-40));
        dbContext.Clientes.Add(cliente);

        await dbContext.SaveChangesAsync();
    }

    public async Task ResetDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvestimentoDbContext>();

        dbContext.Simulacoes.RemoveRange(dbContext.Simulacoes);
        dbContext.PerfisRisco.RemoveRange(dbContext.PerfisRisco);
        await dbContext.SaveChangesAsync();
    }

    public async Task UseDbContextAsync(Func<InvestimentoDbContext, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvestimentoDbContext>();
        await action(dbContext);
    }

    public HttpClient CreateClientSemAuth()
    {
        // Novo client sem Authorization, mas com BaseAddress correto
        return _factory.CreateClient();
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
