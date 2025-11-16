namespace InvestCaixa.Infrastructure.Configurations;

using InvestCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.CPF)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(c => c.DataNascimento)
            .IsRequired();

        builder.Property(c => c.PerfilAtual)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Navigation
        builder.HasMany(c => c.Simulacoes)
            .WithOne(s => s.Cliente)
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.PerfilRisco)
            .WithOne(p => p.Cliente)
            .HasForeignKey<PerfilRisco>(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.CPF).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
