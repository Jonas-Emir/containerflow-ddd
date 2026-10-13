namespace ContainerFlow.Engenharia.Containers;

public interface IConteinerRepository
{
    Task<Conteiner?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conteiner>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conteiner>> ObterDisponiveisAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Conteiner conteiner, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Conteiner conteiner, CancellationToken cancellationToken = default);
}
