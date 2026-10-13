using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.Api.Data.Configurations;

public class PedidoLocacaoConfigurations : IEntityTypeConfiguration<PedidoLocacao>
{
    public void Configure(EntityTypeBuilder<PedidoLocacao> builder)
    {
        builder.ToTable("PedidosLocacao", "vendas");
        builder.HasKey(p => p.Id);
        builder.OwnsOne(s => s.Status, status =>
        {
            status.Property(s => s.Status)
                .HasColumnName("Status")
                .HasConversion<string>();
        });

        builder.OwnsOne(p => p.Localizacao, end =>
        {
            end.Property(e => e.CEP).IsRequired();
            // outras propriedades serão mapeadas por convenção
        });
    }
}
