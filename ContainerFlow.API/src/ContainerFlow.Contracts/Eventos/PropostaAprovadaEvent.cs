using ContainerFlow.DDD;

namespace ContainerFlow.Contracts.Eventos;

public record PropostaAprovadaEvent(
    Guid IdProposta,
    decimal ValorProposta,
    Guid ClienteId,
    Guid SolicitacaoId
) : DomainEvent, IIntegrationEvent
{
    // Construtor sem parâmetros para deserializadores
    public PropostaAprovadaEvent() : this(Guid.Empty, 0, Guid.Empty, Guid.Empty) { }
}
