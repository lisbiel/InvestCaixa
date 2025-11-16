namespace InvestCaixa.Infrastructure.Data;

using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class InvestimentoDbContext : DbContext
{
    public InvestimentoDbContext(DbContextOptions<InvestimentoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<ProdutoInvestimento> Produtos { get; set; } = null!;
    public DbSet<Simulacao> Simulacoes { get; set; } = null!;
    public DbSet<PerfilRisco> PerfisRisco { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas as configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvestimentoDbContext).Assembly);

        // Seed data inicial
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var produtoId1 = Guid.NewGuid();
        var produtoId2 = Guid.NewGuid();
        var produtoId3 = Guid.NewGuid();

        modelBuilder.Entity<ProdutoInvestimento>().HasData(
            new
            {
                Id = produtoId1,
                Nome = "CDB Caixa 2026",
                Tipo = TipoProduto.CDB,
                Rentabilidade = 0.12m,
                Risco = NivelRisco.Baixo,
                PrazoMinimoDias = 180,
                ValorMinimoAplicacao = 1000m,
                PermiteLiquidez = true,
                PerfilRecomendado = PerfilInvestidor.Conservador,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                IsDeleted = false
            },
            new
            {
                Id = produtoId2,
                Nome = "LCI Plus",
                Tipo = TipoProduto.LCI,
                Rentabilidade = 0.11m,
                Risco = NivelRisco.Baixo,
                PrazoMinimoDias = 90,
                ValorMinimoAplicacao = 5000m,
                PermiteLiquidez = false,
                PerfilRecomendado = PerfilInvestidor.Conservador,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                IsDeleted = false
            },
            new
            {
                Id = produtoId3,
                Nome = "Fundo Multimercado XPTO",
                Tipo = TipoProduto.Fundo,
                Rentabilidade = 0.18m,
                Risco = NivelRisco.Alto,
                PrazoMinimoDias = 0,
                ValorMinimoAplicacao = 500m,
                PermiteLiquidez = true,
                PerfilRecomendado = PerfilInvestidor.Agressivo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                IsDeleted = false
            }
        );
    }
}
