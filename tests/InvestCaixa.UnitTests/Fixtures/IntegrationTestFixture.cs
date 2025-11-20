namespace InvestCaixa.UnitTests.Fixtures;

using InvestCaixa.API;
using InvestCaixa.Application.Interfaces;
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

        var clientes = new List<Cliente>
        {
            new ("João Silva", "joao@test.com", "12345678901", DateTime.Now.AddYears(-40)),
            new ("João Sousa", "joaos@test.com", "10987654321", DateTime.Now.AddYears(-20)),
            new ("Maria Santos", "maria@test.com", "11111111111", DateTime.Now.AddYears(-35)),
            new ("Pedro Costa", "pedro@test.com", "22222222222", DateTime.Now.AddYears(-45)),
            new ("Ana Oliveira", "ana@test.com", "33333333333", DateTime.Now.AddYears(-30)),
            new ("Carlos Mendes", "carlos@test.com", "44444444444", DateTime.Now.AddYears(-50)),
            new ("Lucia Ferreira", "lucia@test.com", "55555555555", DateTime.Now.AddYears(-28)),
            new ("Roberto Silva", "roberto@test.com", "66666666666", DateTime.Now.AddYears(-55)),
            new ("Patricia Gomes", "patricia@test.com", "77777777777", DateTime.Now.AddYears(-32)),
            new ("Bruno Alves", "bruno@test.com", "88888888888", DateTime.Now.AddYears(-42)),
            new ("Amanda Rocha", "amanda@test.com", "99999999999", DateTime.Now.AddYears(-25)),
            new ("Felipe Costa", "felipe@test.com", "10101010101", DateTime.Now.AddYears(-38))
        };
        dbContext.Clientes.AddRange(clientes);

        await dbContext.SaveChangesAsync();
    }

    public async Task ResetDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvestimentoDbContext>();

        dbContext.Simulacoes.RemoveRange(dbContext.Simulacoes);
        dbContext.PerfisRisco.RemoveRange(dbContext.PerfisRisco);
        dbContext.PerfisFinanceiros.RemoveRange(dbContext.PerfisFinanceiros);
        dbContext.InvestimentoFinalizados.RemoveRange(dbContext.InvestimentoFinalizados);

        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        await dbContext.SaveChangesAsync();

        // Re-seed the perfis financeiros after reset (for BusinessLogicIntegrationTests)
        var perfilsFinanceiros = new List<PerfilFinanceiro>
        {
            new(1, 2500, 18000, 6000, 2, HorizonteInvestimento.CurtoPrazo, ObjetivoInvestimento.ReservaEmergencia, 0, false)
        };
        dbContext.PerfisFinanceiros.AddRange(perfilsFinanceiros);

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

    /// <summary>
    /// Limpa o cache manualmente (útil entre testes para evitar cache cross-test).
    /// </summary>
    public async Task LimparCacheAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
