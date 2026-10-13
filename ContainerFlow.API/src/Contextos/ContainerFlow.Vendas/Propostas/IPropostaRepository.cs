namespace ContainerFlow.Vendas.Propostas;

public interface IPropostaRepository
{
    Task<Proposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Proposta?> ObterPorIdEPedidoAsync(Guid id, Guid pedidoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proposta>> ObterPorPedidoAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task RemoverAsync(Proposta proposta, CancellationToken cancellationToken = default);
}
