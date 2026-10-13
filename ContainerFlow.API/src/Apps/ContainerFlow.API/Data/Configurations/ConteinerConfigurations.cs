using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class ConteinerConfigurations : IEntityTypeConfiguration<Conteiner>
{
    public void Configure(EntityTypeBuilder<Conteiner> builder)
    {
        builder.ToTable("Conteineres", "engenharia");
        builder.HasKey(c => c.Id);
        builder
            .Property(c => c.Status)
            .HasConversion<string>();
    }
}
