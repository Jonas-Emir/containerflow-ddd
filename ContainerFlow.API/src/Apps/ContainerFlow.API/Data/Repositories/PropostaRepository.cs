using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerFlow.API.Data.Repositories;

public class PropostaRepository : BaseRepository<Proposta>, IPropostaRepository
{
    private readonly AppDbContext _dbContext;

    public PropostaRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Proposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Propostas
            .Include(p => p.Comentarios)
            .Include(p => p.Solicitacao)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Proposta?> ObterPorIdEPedidoAsync(Guid id, Guid pedidoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Propostas
            .Include(p => p.Comentarios)
            .Include(p => p.Solicitacao)
            .FirstOrDefaultAsync(p => p.Id == id && p.SolicitacaoId == pedidoId, cancellationToken);
    }

    public async Task<IReadOnlyList<Proposta>> ObterPorPedidoAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Propostas
            .Include(p => p.Comentarios)
            .Where(p => p.SolicitacaoId == pedidoId)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        await _dbContext.Propostas.AddAsync(proposta, cancellationToken);
    }

    public Task AtualizarAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.ChangeTracker.Entries<Proposta>().FirstOrDefault(e => e.Entity.Id == proposta.Id);
        if (entry == null)
        {
            _dbContext.Propostas.Update(proposta);
        }
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        _dbContext.Propostas.Remove(proposta);
        return Task.CompletedTask;
    }

    public override Task<Proposta?> GetFirstAsync<TProperty>(
        Expression<Func<Proposta, bool>> filtro,
        Expression<Func<Proposta, TProperty>> orderBy,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Propostas
            .Include(p => p.Comentarios)
            .Include(p => p.Solicitacao)
            .OrderBy(orderBy)
            .FirstOrDefaultAsync(filtro, cancellationToken);
    }
}
