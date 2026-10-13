using ContainerFlow.Vendas.Locacoes;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.API.Data.Repositories;

public class LocacaoRepository : BaseRepository<Locacao>, ILocacaoRepository
{
    private readonly AppDbContext _context;

    public LocacaoRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Locacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Locacoes
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Locacao>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Locacoes
            .Where(l => l.ClienteId == clienteId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Locacao>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locacoes
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Locacao locacao, CancellationToken cancellationToken = default)
    {
        await _context.Locacoes.AddAsync(locacao, cancellationToken);
    }

    public Task AtualizarAsync(Locacao locacao, CancellationToken cancellationToken = default)
    {
        var entry = _context.ChangeTracker.Entries<Locacao>().FirstOrDefault(e => e.Entity.Id == locacao.Id);
        if (entry == null)
        {
            _context.Locacoes.Update(locacao);
        }
        return Task.CompletedTask;
    }
}
