namespace ContainerFlow.DDD;

public interface IDomainEvent
{
    Guid EventoId { get; }
    DateTime OcorreuEmUtc { get; }
}
