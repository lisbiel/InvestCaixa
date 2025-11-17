using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestCaixa.Infrastructure.Configurations;

public class InvestimentoFinalizadoConfiguration : IEntityTypeConfiguration<InvestimentoFinalizado>
{
    public void Configure(EntityTypeBuilder<InvestimentoFinalizado> builder)
    {
        builder.ToTable("InvestimentosFinalizados");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ValorAplicado).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.ValorResgatado).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.Status).IsRequired();

        builder.HasOne(i => i.Cliente).WithMany().HasForeignKey(i => i.ClienteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(i => i.ClienteId);
        builder.HasIndex(i => i.Status);
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}