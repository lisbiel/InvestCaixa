using HealthChecks.UI.Client;
using InvestCaixa.API.Extensions;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();
// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InvestimentoDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Create database if it doesn't exist
    dbContext.Database.EnsureCreated();

    // Optional: seed data
    if (!dbContext.Clientes.Any())
    {
        Log.Information("Seeding database with initial data...");
        await SeedDatabase(dbContext, logger);
    }
}

app.UseApiServices();
app.UseTelemetria();

app.MapControllers();

Log.Information("Application started successfully");

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 500,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 500,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 500,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
    }
});

await app.RunAsync();

// Seed initial data
static async Task SeedDatabase(InvestimentoDbContext dbContext, Microsoft.Extensions.Logging.ILogger logger)
{
    try
    {
        // Verificar se já tem dados
        var hasData = await dbContext.Clientes.AnyAsync() || await
        dbContext.Produtos.AnyAsync();
        if (hasData)
        {
            logger.LogInformation("i️ Banco já possui dados, pulando seed");
            return;
        }

        logger.LogInformation(" Iniciando seed do banco de dados...");

        // PRODUTOS REALÍSTICOS
        var produtos = new List<ProdutoInvestimento>
        {
            // CONSERVADORES
            new("CDB Caixa 2026", TipoProduto.CDB, 0.12m, NivelRisco.Baixo, 180, 1000m, true, PerfilInvestidor.Conservador),

            new("LCI Habitação Plus", TipoProduto.LCI, 0.115m, NivelRisco.Baixo, 90, 5000m, false, PerfilInvestidor.Conservador),

            new("Tesouro Selic 2025", TipoProduto.TesouroDireto, 0.105m, NivelRisco.Baixo, 1, 30m, true, PerfilInvestidor.Conservador),

            new("LCA Agronegócio", TipoProduto.LCA, 0.108m, NivelRisco.Baixo, 120, 2000m, false, PerfilInvestidor.Conservador),

            // MODERADOS
            new("CDB Progressivo 2027", TipoProduto.CDB, 0.135m, NivelRisco.Medio, 365,
            10000m, false, PerfilInvestidor.Moderado),
            new("Fundo DI Institucional", TipoProduto.Fundo, 0.125m, NivelRisco.Medio, 30,
            1000m, true, PerfilInvestidor.Moderado),
            new("Tesouro IPCA+ 2030", TipoProduto.TesouroDireto, 0.13m, NivelRisco.Medio,
            365, 50m, true, PerfilInvestidor.Moderado),

            // AGRESSIVOS
            new("Fundo Multimercado Alpha", TipoProduto.Fundo, 0.18m, NivelRisco.Alto, 0,
            500m, true, PerfilInvestidor.Agressivo),
            new("Fundo de Ações Dividendos", TipoProduto.Fundo, 0.22m, NivelRisco.Alto, 0,
            1000m, true, PerfilInvestidor.Agressivo),
            new("CDB High Yield", TipoProduto.CDB, 0.155m, NivelRisco.Alto, 720, 25000m,
            false, PerfilInvestidor.Agressivo)
        };

        await dbContext.Produtos.AddRangeAsync(produtos);

        // CLIENTES DE TESTE DIVERSOS
        var clientes = new List<Cliente>
        {
            new("João Silva Santos", "joao.silva@email.com", "12345678901", new
            DateTime(1985, 3, 15)),
            new("Maria Oliveira Costa", "maria.costa@email.com", "98765432109", new
            DateTime(1990, 7, 22)),
            new("Carlos Pereira Lima", "carlos.lima@email.com", "11122233344", new
            DateTime(1978, 12, 8)),
            new("Ana Beatriz Alves", "ana.alves@email.com", "55566677788", new
            DateTime(1995, 5, 3)),
            new("Roberto Carlos Mendes", "roberto.mendes@email.com", "99988877766", new
            DateTime(1982, 11, 17))
        };

        await dbContext.Clientes.AddRangeAsync(clientes);

        // Salvar primeiro para ter os IDs
        await dbContext.SaveChangesAsync();

        // PERFIS FINANCEIROS REALÍSTICOS
        var perfisFinanceiros = new List<PerfilFinanceiro>
        {
            // Cliente Conservador
            new(clientes[0].Id, 5000m, 80000m, 15000m, 2,
            HorizonteInvestimento.CurtoPrazo, ObjetivoInvestimento.ReservaEmergencia,
            1, false),

            // Cliente Moderado
            new(clientes[1].Id, 8500m, 150000m, 25000m, 1,
            HorizonteInvestimento.MedioPrazo, ObjetivoInvestimento.CompraImovel,
            4, true),

            // Cliente Agressivo
            new(clientes[2].Id, 15000m, 500000m, 50000m, 0,
            HorizonteInvestimento.LongoPrazo, ObjetivoInvestimento.Aposentadoria,
            9, true),

            // Cliente Moderado Jovem
            new(clientes[3].Id, 4000m, 30000m, 5000m, 1,
            HorizonteInvestimento.LongoPrazo, ObjetivoInvestimento.EducacaoFilhos,
            5, false),

            // Cliente Conservador Experiente
            new(clientes[4].Id, 12000m, 350000m, 80000m, 3,
            HorizonteInvestimento.MedioPrazo, ObjetivoInvestimento.Aposentadoria,
            3, true)
        };

        await dbContext.PerfisFinanceiros.AddRangeAsync(perfisFinanceiros);

        // SIMULAÇÕES DE EXEMPLO
        var simulacoes = new List<Simulacao>
            {
                // João - Conservador
                new(clientes[0].Id, produtos[0].Nome, produtos[0].Id, 10000m, 11268.25m, 12, DateTime.UtcNow.AddDays(-15))
                ,
                new(clientes[0].Id, produtos[2].Nome, produtos[2].Id, 5000m, 5252.36m, 6,  DateTime.UtcNow.AddDays(-8)),

                // Maria - Moderado
                new(clientes[1].Id, produtos[1].Nome, produtos[5].Id, 25000m, 31080.63m, 24, DateTime.UtcNow.AddDays(-20)),
                new(clientes[1].Id,produtos[6].Nome, produtos[6].Id, 15000m, 21643.45m, 36, DateTime.UtcNow.AddDays(-5)),

                // Carlos - Agressivo
                new(clientes[2].Id, produtos[7].Nome, produtos[7].Id, 50000m, 82151.60m, 36, DateTime.UtcNow.AddDays(-30)),
                new(clientes[2].Id, produtos[8].Nome, produtos[8].Id, 100000m, 181584.80m, 60, DateTime.UtcNow.AddDays(-12)),

                // Ana - Moderado Jovem
                new(clientes[3].Id, produtos[2].Nome, produtos[2].Id, 5000m, 5252.36m, 6,  DateTime.UtcNow.AddDays(-7)),

                // Roberto - Conservador Experiente
                new(clientes[4].Id, produtos[1].Nome, produtos[1].Id, 100000m, 122000m, 18, DateTime.UtcNow.AddDays(-25))
            };

        await dbContext.Simulacoes.AddRangeAsync(simulacoes);

        // INVESTIMENTOS FINALIZADOS - HISTÓRICO REAL DE INVESTIMENTOS

        var investimentosFinalizados = new List<InvestimentoFinalizado>
        {
            // JOÃO SILVA - PERFIL CONSERVADOR (Histórico consistente)
            new(clientes[0].Id, produtos[0].Id, 15000m, DateTime.UtcNow.AddMonths(-8), 12, DateTime.UtcNow, StatusInvestimento.Resgatado, 16530m),
            new(clientes[0].Id, produtos[2].Id, 8000m, DateTime.UtcNow.AddMonths(-6), 6, DateTime.UtcNow, StatusInvestimento.Resgatado, 8336m),
            new(clientes[0].Id, produtos[1].Id, 20000m, DateTime.UtcNow.AddMonths(-15), 12, DateTime.UtcNow.AddMonths(-3), StatusInvestimento.Resgatado, 22300m),

            // MARIA COSTA - PERFIL MODERADO (Diversificação)
            new(clientes[1].Id, produtos[4].Id, 30000m, DateTime.UtcNow.AddMonths(-12), 24),
            new(clientes[1].Id, produtos[5].Id, 12000m, DateTime.UtcNow.AddMonths(-9), 18),
            new(clientes[1].Id, produtos[0].Id, 18000m, DateTime.UtcNow.AddMonths(-18), 12, DateTime.UtcNow.AddMonths(-6), StatusInvestimento.Resgatado, 19756m),

            // CARLOS LIMA - PERFIL AGRESSIVO (Alto volume, alto risco)
            new(clientes[2].Id, produtos[7].Id, 75000m, DateTime.UtcNow.AddMonths(-24), 36),
            new(clientes[2].Id, produtos[8].Id, 50000m, DateTime.UtcNow.AddMonths(-15), 24),
            new(clientes[2].Id, produtos[9].Id, 100000m,DateTime.UtcNow.AddMonths(-36), 36, DateTime.UtcNow, StatusInvestimento.Resgatado, 131850m),

            // ANA ALVES - PERFIL MODERADO JOVEM (Começando a investir)
            new(clientes[3].Id, produtos[2].Id, 3000m, DateTime.UtcNow.AddMonths(-8), 6, DateTime.UtcNow.AddMonths(-2), StatusInvestimento.Resgatado, 3126m),
            new(clientes[3].Id, produtos[5].Id, 8000m, DateTime.UtcNow.AddMonths(-6), 12),

            // ROBERTO MENDES - CONSERVADOR EXPERIENTE (Alto volume conservador)
            new(clientes[4].Id, produtos[1].Id, 150000m, DateTime.UtcNow.AddMonths(-20), 18, DateTime.UtcNow.AddMonths(-2), StatusInvestimento.Resgatado, 175875m),
            new(clientes[4].Id, produtos[3].Id, 80000m, DateTime.UtcNow.AddMonths(-12), 24),
            new(clientes[4].Id, produtos[0].Id, 60000m, DateTime.UtcNow.AddMonths(-24), 12, DateTime.UtcNow.AddMonths(-12), StatusInvestimento.Resgatado, 65760m),
            new(clientes[4].Id, produtos[4].Id, 120000m, DateTime.UtcNow.AddMonths(-6), 36)
        };

        await
        dbContext.InvestimentoFinalizados.AddRangeAsync(investimentosFinalizados);

        // Salvar tudo
        var savedCount = await dbContext.SaveChangesAsync();

        logger.LogInformation(" Seed concluído com sucesso!");
        logger.LogInformation(" Dados criados:");
        logger.LogInformation(" {ProdutoCount} produtos de investimento",
        produtos.Count);
        logger.LogInformation(" {ClienteCount} clientes", clientes.Count);
        logger.LogInformation(" {PerfilCount} perfis financeiros", perfisFinanceiros.Count);
        logger.LogInformation(" {SimulacaoCount} simulações", simulacoes.Count);
        logger.LogInformation(" {InvestimentoCount} investimentos finalizados",
        investimentosFinalizados.Count);
        logger.LogInformation(" Total de registros salvos: {SavedCount}", savedCount);

        // ESTATÍSTICAS INTERESSANTES DOS INVESTIMENTOS
        var totalAplicado = investimentosFinalizados.Sum(i => i.ValorAplicado);

        logger.LogInformation(" Estatísticas dos investimentos:");
        logger.LogInformation(" Volume total aplicado: R$ {TotalAplicado:N2}",
        totalAplicado);

        var investimentosAtivos = investimentosFinalizados.Count(i => i.Status ==
        StatusInvestimento.Ativo);
        var investimentosFinalizadosCount = investimentosFinalizados.Count(i => i.Status ==
        StatusInvestimento.Resgatado);

        logger.LogInformation(" Investimentos ativos: {Ativos}", investimentosAtivos);
        logger.LogInformation(" Investimentos finalizados: {Finalizados}",
        investimentosFinalizadosCount);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, " Erro durante seed do banco de dados");
        throw;
    }
}

[ExcludeFromCodeCoverage]
public partial class Program { }
