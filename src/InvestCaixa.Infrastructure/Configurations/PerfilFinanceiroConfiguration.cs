using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCaixa.Infrastructure.Configurations;

public class PerfilFinanceiroConfiguration : IEntityTypeConfiguration<PerfilFinanceiro>
{
    public void Configure(EntityTypeBuilder<PerfilFinanceiro> builder)
    {
        builder.ToTable("PerfisFinanceiros");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RendaMensal).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.PatrimonioTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.DividasAtivas).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.ToleranciaPerda).IsRequired();
        builder.Property(p => p.Horizonte).IsRequired();
        builder.Property(p => p.Objetivo).IsRequired();

        builder.HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.ClienteId);
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
