namespace ContainerFlow.Clientes.Cadastro;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
