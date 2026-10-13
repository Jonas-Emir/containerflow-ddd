using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerFlow.API.Data.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    private readonly AppDbContext _dbContext;

    public ClienteRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clientes
            .Include(c => c.Enderecos)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }

    public Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<EnderecoCli>()
            .Where(e => e.ClienteId == cliente.Id && !cliente.Enderecos.Contains(e))
            .ToList()
            .ForEach(e => _dbContext.Set<EnderecoCli>().Remove(e));

        var entry = _dbContext.ChangeTracker.Entries<Cliente>().FirstOrDefault(e => e.Entity.Id == cliente.Id);
        if (entry == null)
        {
            _dbContext.Clientes.Update(cliente);
        }
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _dbContext.Clientes.Remove(cliente);
        return Task.CompletedTask;
    }

    public override async Task<Cliente?> GetFirstAsync<TProperty>(
        Expression<Func<Cliente, bool>> filtro,
        Expression<Func<Cliente, TProperty>> orderBy,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clientes
            .Include(c => c.Enderecos)
            .OrderBy(orderBy)
            .FirstOrDefaultAsync(filtro, cancellationToken);
    }
}
