using ContainerFlow.Contracts;
using ContainerFlow.Contracts.Eventos;

namespace ContainerFlow.Engenharia.Containers;

public class ReservadorDeConteiner
{
    private readonly IConteinerRepository _repoConteiner;
    private readonly IEventoManager _eventoManager;
    private readonly IUnitOfWork _unitOfWork;

    public ReservadorDeConteiner(
        IConteinerRepository repoConteiner,
        IEventoManager eventoManager,
        IUnitOfWork unitOfWork)
    {
        _repoConteiner = repoConteiner;
        _eventoManager = eventoManager;
        _unitOfWork = unitOfWork;
    }

    public Task ExecutarAsync() => ExecutarAsync(CancellationToken.None);

    public async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        await _eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            nameof(PropostaAprovadaEvent),
            nameof(ReservadorDeConteiner),
            async (mensagem, ct) =>
            {
                var disponiveis = await _repoConteiner.ObterDisponiveisAsync(ct);
                var conteiner = disponiveis.FirstOrDefault();
                if (conteiner is not null)
                {
                    conteiner.ReservarParaLocacao(mensagem.Corpo.IdProposta);
                    await _repoConteiner.AtualizarAsync(conteiner, ct);
                    await _unitOfWork.CommitAsync(ct);
                }
            },
            cancellationToken);

        // Suporte retrocompativel
        await _eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            "PropostaAprovada",
            nameof(ReservadorDeConteiner),
            async (mensagem, ct) =>
            {
                var disponiveis = await _repoConteiner.ObterDisponiveisAsync(ct);
                var conteiner = disponiveis.FirstOrDefault();
                if (conteiner is not null)
                {
                    conteiner.ReservarParaLocacao(mensagem.Corpo.IdProposta);
                    await _repoConteiner.AtualizarAsync(conteiner, ct);
                    await _unitOfWork.CommitAsync(ct);
                }
            },
            cancellationToken);
    }
}
