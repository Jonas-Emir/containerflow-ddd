using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerFlow.API.Data.Repositories;

public class SolicitacaoRepository : BaseRepository<PedidoLocacao>, ISolicitacaoRepository
{
    private readonly AppDbContext _dbContext;

    public SolicitacaoRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PedidoLocacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pedidos
            .Include(s => s.Propostas)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PedidoLocacao>> ObterAtivasPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pedidos
            .Include(s => s.Propostas)
            .Where(s => s.ClienteId == clienteId && s.Status.Status == "Ativa")
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default)
    {
        await _dbContext.Pedidos.AddAsync(pedido, cancellationToken);
    }

    public Task AtualizarAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.ChangeTracker.Entries<PedidoLocacao>().FirstOrDefault(e => e.Entity.Id == pedido.Id);
        if (entry == null)
        {
            _dbContext.Pedidos.Update(pedido);
        }
        return Task.CompletedTask;
    }

    public Task RemoverAsync(PedidoLocacao pedido, CancellationToken cancellationToken = default)
    {
        _dbContext.Pedidos.Remove(pedido);
        return Task.CompletedTask;
    }

    public override Task<PedidoLocacao?> GetFirstAsync<TProperty>(
        Expression<Func<PedidoLocacao, bool>> filtro,
        Expression<Func<PedidoLocacao, TProperty>> orderBy,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Pedidos
            .Include(s => s.Propostas)
            .OrderBy(orderBy)
            .FirstOrDefaultAsync(filtro, cancellationToken);
    }
}
