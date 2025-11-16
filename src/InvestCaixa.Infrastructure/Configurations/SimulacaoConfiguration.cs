namespace InvestCaixa.Infrastructure.Configurations;

using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SimulacaoConfiguration : IEntityTypeConfiguration<Simulacao>
{
    public void Configure(EntityTypeBuilder<Simulacao> builder)
    {
        builder.ToTable("Simulacoes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ValorInvestido)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.ValorFinal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.RentabilidadeCalculada)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(s => s.DataSimulacao)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne(s => s.Cliente)
            .WithMany(c => c.Simulacoes)
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Produto)
            .WithMany()
            .HasForeignKey(s => s.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(s => s.ClienteId);
        builder.HasIndex(s => s.ProdutoId);
        builder.HasIndex(s => s.DataSimulacao);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
