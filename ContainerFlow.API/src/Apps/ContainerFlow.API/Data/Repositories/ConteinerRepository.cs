using ContainerFlow.Engenharia.Containers;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.API.Data.Repositories;

public class ConteinerRepository : BaseRepository<Conteiner>, IConteinerRepository
{
    private readonly AppDbContext _dbContext;

    public ConteinerRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conteiner?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conteineres
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Conteiner>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conteineres
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conteiner>> ObterDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conteineres
            .Where(c => c.LocacaoId == Guid.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Conteiner conteiner, CancellationToken cancellationToken = default)
    {
        await _dbContext.Conteineres.AddAsync(conteiner, cancellationToken);
    }

    public Task AtualizarAsync(Conteiner conteiner, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.ChangeTracker.Entries<Conteiner>().FirstOrDefault(e => e.Entity.Id == conteiner.Id);
        if (entry == null)
        {
            _dbContext.Conteineres.Update(conteiner);
        }
        return Task.CompletedTask;
    }
}
