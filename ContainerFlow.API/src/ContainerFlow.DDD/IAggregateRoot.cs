namespace ContainerFlow.DDD;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> Eventos { get; }
    void LimparEventos();
}
