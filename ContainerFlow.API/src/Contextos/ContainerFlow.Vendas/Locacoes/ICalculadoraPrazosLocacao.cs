using ContainerFlow.Vendas.Propostas;

namespace ContainerFlow.Vendas.Locacoes;

public interface ICalculadoraPrazosLocacao
{
    DateTime CalculaDataPrevistaParaEntrega(Proposta proposta);
    DateTime CalculaDataPrevistaParaTermino(Proposta proposta);
}

public class CalculadoraPadraoPrazosLocacao : ICalculadoraPrazosLocacao
{
    public DateTime CalculaDataPrevistaParaEntrega(Proposta proposta)
    {
        ArgumentNullException.ThrowIfNull(proposta);
        if (proposta.Solicitacao is null)
            throw new InvalidOperationException("A proposta deve conter os dados da solicitação para calcular prazos.");

        return proposta.Solicitacao
            .DataInicioOperacao
            .AddDays(-proposta.Solicitacao.DisponibilidadePrevia);
    }

    public DateTime CalculaDataPrevistaParaTermino(Proposta proposta)
    {
        ArgumentNullException.ThrowIfNull(proposta);
        if (proposta.Solicitacao is null)
            throw new InvalidOperationException("A proposta deve conter os dados da solicitação para calcular prazos.");

        return proposta.Solicitacao
            .DataInicioOperacao
            .AddDays(proposta.Solicitacao.DuracaoPrevistaLocacao);
    }
}