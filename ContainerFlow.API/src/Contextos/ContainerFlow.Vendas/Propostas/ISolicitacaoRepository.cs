namespace ContainerFlow.Vendas.Propostas;

public interface ISolicitacaoRepository
{
    Task<PedidoLocacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PedidoLocacao>> ObterAtivasPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default);
    Task AtualizarAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default);
    Task RemoverAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default);
}
