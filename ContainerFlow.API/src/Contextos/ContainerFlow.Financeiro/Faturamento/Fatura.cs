using ContainerFlow.DDD;

namespace ContainerFlow.Financeiro.Faturamento;

public enum StatusFatura
{
    Pendente,
    Paga,
    Cancelada
}

public class Fatura : AggregateRoot<Guid>
{
    public Fatura()
    {
        Id = Guid.NewGuid();
        Numero = string.Empty;
        Status = StatusFatura.Pendente;
        DataEmissao = DateTime.UtcNow;
    }

    public Fatura(string numero, decimal total, Guid locacaoId, DateTime dataEmissao, DateTime dataVencimento)
    {
        Id = Guid.NewGuid();
        Numero = numero;
        Total = total;
        LocacaoId = locacaoId;
        DataEmissao = dataEmissao;
        DataVencimento = dataVencimento;
        Status = StatusFatura.Pendente;
    }

    public string Numero { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public Guid LocacaoId { get; set; }
    public decimal Total { get; set; }
    public StatusFatura Status { get; set; } = StatusFatura.Pendente;

    public void MarcarComoPaga()
    {
        if (Status == StatusFatura.Cancelada)
            throw new DomainException("Não é possível pagar uma fatura cancelada.");

        Status = StatusFatura.Paga;
    }

    public void Cancelar()
    {
        if (Status == StatusFatura.Paga)
            throw new DomainException("Não é possível cancelar uma fatura já paga.");

        Status = StatusFatura.Cancelada;
    }
}
