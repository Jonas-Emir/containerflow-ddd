using ContainerFlow.Financeiro.Faturamento;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.API.Data.Repositories;

public class FaturaRepository : BaseRepository<Fatura>, IFaturaRepository
{
    private readonly AppDbContext _dbContext;

    public FaturaRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Fatura?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Faturas
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Fatura>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Faturas
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Fatura>> ObterPorLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Faturas
            .Where(f => f.LocacaoId == locacaoId)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Fatura fatura, CancellationToken cancellationToken = default)
    {
        await _dbContext.Faturas.AddAsync(fatura, cancellationToken);
    }

    public Task AtualizarAsync(Fatura fatura, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.ChangeTracker.Entries<Fatura>().FirstOrDefault(e => e.Entity.Id == fatura.Id);
        if (entry == null)
        {
            _dbContext.Faturas.Update(fatura);
        }
        return Task.CompletedTask;
    }
}
