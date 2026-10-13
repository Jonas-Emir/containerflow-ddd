using ContainerFlow.Contracts;
using ContainerFlow.Vendas.Locacoes;

namespace ContainerFlow.Vendas.Propostas;

public interface IPropostaService
{
    Task<Proposta?> AprovarAsync(AprovarProposta comando, CancellationToken cancellationToken = default);
    Task<Proposta?> ComentarAsync(ComentarProposta comando, CancellationToken cancellationToken = default);
}

public class PropostaService : IPropostaService
{
    private readonly IPropostaRepository _repoProposta;
    private readonly ILocacaoRepository _repoLocacao;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculadoraPrazosLocacao _calculadora;

    public PropostaService(
        IPropostaRepository repoProposta,
        ILocacaoRepository repoLocacao,
        IUnitOfWork unitOfWork,
        ICalculadoraPrazosLocacao calculadora)
    {
        _repoProposta = repoProposta;
        _repoLocacao = repoLocacao;
        _unitOfWork = unitOfWork;
        _calculadora = calculadora;
    }

    public async Task<Proposta?> AprovarAsync(AprovarProposta comando, CancellationToken cancellationToken = default)
    {
        var proposta = await _repoProposta.ObterPorIdEPedidoAsync(comando.IdProposta, comando.IdPedido, cancellationToken);
        if (proposta is null) return null;

        if (proposta.Aprovar())
        {
            var locacao = new Locacao
            {
                Id = Guid.NewGuid(),
                PropostaId = proposta.Id,
                ClienteId = proposta.ClienteId,
                DataInicio = DateTime.UtcNow,
                DataPrevistaEntrega = _calculadora.CalculaDataPrevistaParaEntrega(proposta),
                DataTermino = _calculadora.CalculaDataPrevistaParaTermino(proposta)
            };

            await _repoProposta.AtualizarAsync(proposta, cancellationToken);
            await _repoLocacao.AdicionarAsync(locacao, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return proposta;
    }

    public async Task<Proposta?> ComentarAsync(ComentarProposta comando, CancellationToken cancellationToken = default)
    {
        var proposta = await _repoProposta.ObterPorIdEPedidoAsync(comando.IdProposta, comando.IdPedido, cancellationToken);
        if (proposta is null) return null;

        proposta.AddComentario(new Comentario
        {
            Id = Guid.NewGuid(),
            Data = DateTime.UtcNow,
            Usuario = comando.Pessoa,
            Texto = comando.Mensagem
        });

        await _repoProposta.AtualizarAsync(proposta, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return proposta;
    }
}
