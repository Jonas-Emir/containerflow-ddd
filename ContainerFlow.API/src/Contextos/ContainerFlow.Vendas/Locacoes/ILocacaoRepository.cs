namespace ContainerFlow.Vendas.Locacoes;

public interface ILocacaoRepository
{
    Task<Locacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Locacao>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Locacao>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Locacao locacao, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Locacao locacao, CancellationToken cancellationToken = default);
}
