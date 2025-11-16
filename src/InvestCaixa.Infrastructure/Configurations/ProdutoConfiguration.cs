namespace InvestCaixa.Infrastructure.Configurations;

using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProdutoConfiguration : IEntityTypeConfiguration<ProdutoInvestimento>
{
    public void Configure(EntityTypeBuilder<ProdutoInvestimento> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Rentabilidade)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(p => p.Risco)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ValorMinimoAplicacao)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PerfilRecomendado)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(p => p.Tipo);
        builder.HasIndex(p => p.PerfilRecomendado);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
