using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class PropostaConfigurations : IEntityTypeConfiguration<Proposta>
{
    public void Configure(EntityTypeBuilder<Proposta> builder)
    {
        builder.ToTable("Propostas", "vendas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ValorTotal)
            .HasConversion(
                v => v != null ? v.Valor : 0m,
                v => new ValorMonetario(v))
            .HasColumnType("decimal(18,2)");

        builder.OwnsOne(p => p.Situacao, status =>
        {
            status.Property(s => s.Status)
                .HasColumnName("Status")
                .HasConversion<string>();
        });

        builder.HasOne(p => p.Solicitacao)
            .WithMany(s => s.Propostas)
            .HasForeignKey(p => p.SolicitacaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
