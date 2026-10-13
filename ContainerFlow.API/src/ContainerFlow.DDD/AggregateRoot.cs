namespace ContainerFlow.DDD;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<IDomainEvent> _eventos = [];

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();

    protected void AdicionarEvento(IDomainEvent evento)
    {
        _eventos.Add(evento);
    }

    public void LimparEventos()
    {
        _eventos.Clear();
    }
}
