namespace ContainerFlow.DDD;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventoId { get; init; } = Guid.NewGuid();
    public DateTime OcorreuEmUtc { get; init; } = DateTime.UtcNow;

    protected DomainEvent() { }

    protected DomainEvent(Guid eventoId, DateTime ocorreuEmUtc)
    {
        EventoId = eventoId;
        OcorreuEmUtc = ocorreuEmUtc;
    }
}
