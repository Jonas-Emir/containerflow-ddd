using ContainerFlow.DDD;

namespace ContainerFlow.Contracts;

public interface IIntegrationEvent : IDomainEvent
{
    Guid CorrelationId => EventoId;
}
