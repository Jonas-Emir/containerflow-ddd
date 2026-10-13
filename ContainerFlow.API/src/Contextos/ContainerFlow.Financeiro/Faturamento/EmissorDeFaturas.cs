using ContainerFlow.Contracts;
using ContainerFlow.Contracts.Eventos;

namespace ContainerFlow.Financeiro.Faturamento;

public class EmissorDeFaturas
{
    private readonly IFaturaRepository _repoFatura;
    private readonly IEventoManager _eventoManager;
    private readonly IUnitOfWork _unitOfWork;

    public EmissorDeFaturas(
        IFaturaRepository repoFatura,
        IEventoManager eventoManager,
        IUnitOfWork unitOfWork)
    {
        _repoFatura = repoFatura;
        _eventoManager = eventoManager;
        _unitOfWork = unitOfWork;
    }

    public Task ExecutarAsync() => ExecutarAsync(CancellationToken.None);

    public async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        await _eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            nameof(PropostaAprovadaEvent),
            nameof(EmissorDeFaturas),
            async (mensagem, ct) =>
            {
                var fatura = new Fatura
                {
                    Id = Guid.NewGuid(),
                    DataEmissao = DateTime.UtcNow,
                    DataVencimento = DateTime.UtcNow.AddDays(5),
                    Numero = $"FAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
                    Total = mensagem.Corpo.ValorProposta
                };

                await _repoFatura.AdicionarAsync(fatura, ct);
                await _unitOfWork.CommitAsync(ct);
            },
            cancellationToken);

        // Suporte retrocompatível caso haja eventos gravados com o nome 'PropostaAprovada'
        await _eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            "PropostaAprovada",
            nameof(EmissorDeFaturas),
            async (mensagem, ct) =>
            {
                var fatura = new Fatura
                {
                    Id = Guid.NewGuid(),
                    DataEmissao = DateTime.UtcNow,
                    DataVencimento = DateTime.UtcNow.AddDays(5),
                    Numero = $"FAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
                    Total = mensagem.Corpo.ValorProposta
                };

                await _repoFatura.AdicionarAsync(fatura, ct);
                await _unitOfWork.CommitAsync(ct);
            },
            cancellationToken);
    }
}
