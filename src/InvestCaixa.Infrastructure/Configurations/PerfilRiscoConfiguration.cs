namespace InvestCaixa.Infrastructure.Configurations;

using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PerfilRiscoConfiguration : IEntityTypeConfiguration<PerfilRisco>
{
    public void Configure(EntityTypeBuilder<PerfilRisco> builder)
    {
        builder.ToTable("PerfisRisco");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Perfil)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Descricao)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.VolumeInvestimentos)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Foreign key
        builder.HasOne(p => p.Cliente)
            .WithOne(c => c.PerfilRisco)
            .HasForeignKey<PerfilRisco>(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => p.ClienteId).IsUnique();
        builder.HasIndex(p => p.Perfil);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
