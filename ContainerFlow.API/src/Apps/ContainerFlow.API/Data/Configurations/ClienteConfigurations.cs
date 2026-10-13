using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class ClienteConfigurations : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes", "clientes");
        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.Nome).IsRequired();
        
        builder
            .OwnsOne(c => c.Email, cfg =>
            {
                cfg.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired();
            });

        builder
            .Property(c => c.CPF).IsRequired();
    }
}
