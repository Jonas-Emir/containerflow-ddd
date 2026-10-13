using ContainerFlow.Api.Eventos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class OutboxMessageConfigurations : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "eventos");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CorrelationId)
            .IsRequired();

        builder.Property(o => o.TipoEvento)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.InfoEvento)
            .IsRequired();

        builder.Property(o => o.DataCriacaoUtc)
            .IsRequired();

        builder.Property(o => o.UltimoErro)
            .HasMaxLength(1000);
    }
}
