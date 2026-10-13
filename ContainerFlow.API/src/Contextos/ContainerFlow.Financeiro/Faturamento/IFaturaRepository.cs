namespace ContainerFlow.Financeiro.Faturamento;

public interface IFaturaRepository
{
    Task<Fatura?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Fatura>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Fatura>> ObterPorLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Fatura fatura, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Fatura fatura, CancellationToken cancellationToken = default);
}
