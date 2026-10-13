using ContainerFlow.DDD;
using ContainerFlow.Vendas.Propostas;

namespace ContainerFlow.Vendas.Locacoes;

public record StatusLocacao(string Status)
{
    public static StatusLocacao Contrato => new("Contrato");
    public static StatusLocacao Faturamento => new("Faturamento");
    public static StatusLocacao Finalizada => new("Finalizada");
    public static StatusLocacao Cancelada => new("Cancelada");
    public override string ToString() => Status;
    public static StatusLocacao? Parse(string status)
    {
        return status switch
        {
            "Contrato" => Contrato,
            "Faturamento" => Faturamento,
            "Finalizada" => Finalizada,
            "Cancelada" => Cancelada,
            _ => null
        };
    }
}

public class Locacao : AggregateRoot<Guid>
{
    public Locacao()
    {
        Id = Guid.NewGuid();
        DataInicio = DateTime.UtcNow;
    }

    public Guid PropostaId { get; set; }
    public Guid ClienteId { get; set; }
    public Proposta? Proposta { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataPrevistaEntrega { get; set; }
    public DateTime DataTermino { get; set; }
    public StatusLocacao Status { get; set; } = StatusLocacao.Contrato;

    public void AvancarParaFaturamento()
    {
        Status = StatusLocacao.Faturamento;
    }

    public void Finalizar()
    {
        Status = StatusLocacao.Finalizada;
    }

    public void Cancelar()
    {
        Status = StatusLocacao.Cancelada;
    }
}
