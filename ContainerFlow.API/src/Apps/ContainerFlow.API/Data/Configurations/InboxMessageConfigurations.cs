using ContainerFlow.Api.Eventos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class InboxMessageConfigurations : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages", "eventos");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TipoLeitor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.DataProcessamentoUtc)
            .IsRequired();

        builder.Property(i => i.Sucesso)
            .IsRequired();

        builder.Property(i => i.Erro)
            .HasMaxLength(1000);

        builder.HasIndex(i => new { i.OutboxMessageId, i.TipoLeitor })
            .IsUnique();

        builder.HasOne(i => i.Evento)
            .WithMany()
            .HasForeignKey(i => i.OutboxMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
