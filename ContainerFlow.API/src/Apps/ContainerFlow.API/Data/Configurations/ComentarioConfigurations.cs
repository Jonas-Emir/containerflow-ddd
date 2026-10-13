using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerFlow.API.Data.Configurations;

public class ComentarioConfigurations : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentarios", "vendas");
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.Proposta)
            .WithMany(p => p.Comentarios)
            .HasForeignKey(c => c.PropostaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
