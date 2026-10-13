using ContainerFlow.Vendas.Locacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class LocacaoConfigurations : IEntityTypeConfiguration<Locacao>
{
    public void Configure(EntityTypeBuilder<Locacao> builder)
    {
        builder.ToTable("Locacoes", "vendas");
        builder.HasKey(l => l.Id);
        builder.OwnsOne(locacao => locacao.Status, status =>
        {
            status.Property(s => s.Status)
                .HasColumnName("Status")
                .HasConversion<string>();
        });

        builder.HasOne(l => l.Proposta)
            .WithOne()
            .HasForeignKey<Locacao>(l => l.PropostaId);
    }
}
