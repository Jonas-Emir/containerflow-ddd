using ContainerFlow.Contracts.Eventos;

namespace ContainerFlow.Vendas.Propostas;

public record PropostaAprovada(
    Guid IdProposta,
    decimal ValorProposta,
    Guid ClienteId = default,
    Guid SolicitacaoId = default
) : PropostaAprovadaEvent(IdProposta, ValorProposta, ClienteId, SolicitacaoId);
