using ContainerFlow.Api.Eventos;
using ContainerFlow.Contracts;
using ContainerFlow.DDD;
using ContainerFlow.Financeiro.Faturamento;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ContainerFlow.API.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PedidoLocacao> Pedidos { get; set; }
    public DbSet<Proposta> Propostas { get; set; }
    public DbSet<Locacao> Locacoes { get; set; }
    public DbSet<Conteiner> Conteineres { get; set; }
    public DbSet<Fatura> Faturas { get; set; }
    public DbSet<OutboxMessage> Outbox { get; set; }
    public DbSet<InboxMessage> Inbox { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.Eventos.ToList();
                entity.LimparEventos();
                return events;
            })
            .ToList();

        var outboxMessages = domainEvents
            .Select(@event => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CorrelationId = @event.EventoId,
                TipoEvento = @event.GetType().Name,
                InfoEvento = JsonSerializer.Serialize(@event, @event.GetType()),
                DataCriacaoUtc = @event.OcorreuEmUtc,
            })
            .ToList();

        Outbox.AddRange(outboxMessages);

        return base.SaveChangesAsync(cancellationToken);
    }
}
