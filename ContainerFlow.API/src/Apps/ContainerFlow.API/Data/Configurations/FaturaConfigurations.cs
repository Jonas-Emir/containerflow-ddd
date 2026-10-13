using ContainerFlow.Financeiro.Faturamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class FaturaConfigurations : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Faturas", "financeiro");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Numero)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Total)
            .HasColumnType("decimal(18,2)");
    }
}
