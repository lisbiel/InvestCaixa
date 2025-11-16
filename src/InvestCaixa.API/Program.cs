using InvestCaixa.API.Extensions;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
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

    // Create database if it doesn't exist
    dbContext.Database.EnsureCreated();

    // Optional: seed data
    if (!dbContext.Clientes.Any())
    {
        Log.Information("Seeding database with initial data...");
        await SeedDatabase(dbContext);
    }
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiServices();
app.UseTelemetria();

app.MapControllers();

Log.Information("Application started successfully");

await app.RunAsync();

// Seed initial data
static async Task SeedDatabase(InvestimentoDbContext dbContext)
{
    try
    {
        // Add default products
        var cdbId = Guid.NewGuid();
        var lciId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();

        var produtos = new List<ProdutoInvestimento>
        {
            new("CDB Caixa 2026", TipoProduto.CDB, 0.12m, NivelRisco.Baixo, 180, 1000m, true, PerfilInvestidor.Conservador),
            new("LCI Plus", TipoProduto.LCI, 0.11m, NivelRisco.Baixo, 90, 5000m, false, PerfilInvestidor.Conservador),
            new("Fundo Multimercado", TipoProduto.Fundo, 0.18m, NivelRisco.Alto, 0, 500m, true, PerfilInvestidor.Agressivo),
            new("Tesouro IPCA+ 2035", TipoProduto.TesouroDireto, 0.06m, NivelRisco.Baixo, 365, 30m, true, PerfilInvestidor.Conservador),
            new("LCA Desenvolvimento", TipoProduto.LCA, 0.10m, NivelRisco.Baixo, 120, 2000m, false, PerfilInvestidor.Moderado)
        };

        dbContext.Produtos.AddRange(produtos);

        // Add default client
        var cliente = new Cliente("João Silva", "joao@test.com", "12345678901", DateTime.Now.AddYears(-40));
        dbContext.Clientes.Add(cliente);

        await dbContext.SaveChangesAsync();

        Log.Information("Database seeded successfully with {ProdutoCount} products and 1 client", produtos.Count);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error seeding database");
    }
}

[ExcludeFromCodeCoverage]
public partial class Program { }
