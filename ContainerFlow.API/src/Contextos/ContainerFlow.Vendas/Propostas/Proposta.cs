using ContainerFlow.Contracts.Eventos;
using ContainerFlow.DDD;

namespace ContainerFlow.Vendas.Propostas;

public record SituacaoProposta(string Status)
{
    public static SituacaoProposta Enviada => new("Enviada");
    public static SituacaoProposta Expirada => new("Expirada");
    public static SituacaoProposta Aceita => new("Aceita");
    public static SituacaoProposta Recusada => new("Recusada");
    public static SituacaoProposta Cancelada => new("Cancelada");
    public override string ToString() => Status;
    public static SituacaoProposta? Parse(string status)
    {
        return status switch
        {
            "Enviada" => Enviada,
            "Expirada" => Expirada,
            "Aceita" => Aceita,
            "Recusada" => Recusada,
            "Cancelada" => Cancelada,
            _ => null
        };
    }
}

public record ValorMonetario
{
    public ValorMonetario(decimal valor)
    {
        if (valor < 0) valor = 0;
        Valor = valor;
    }

    public decimal Valor { get; }
}

public class Proposta : AggregateRoot<Guid>
{
    public Proposta()
    {
        Id = Guid.NewGuid();
        NomeArquivo = string.Empty;
        ValorTotal = new ValorMonetario(0);
        DataCriacao = DateTime.UtcNow;
    }

    public SituacaoProposta Situacao { get; set; } = SituacaoProposta.Enviada;
    public ValorMonetario ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataExpiracao { get; set; }
    public string NomeArquivo { get; set; }
    public Guid ClienteId { get; set; }
    public Guid SolicitacaoId { get; set; }
    public PedidoLocacao? Solicitacao { get; set; }
    public ICollection<Comentario> Comentarios { get; } = new List<Comentario>();

    public Comentario AddComentario(Comentario comentario)
    {
        if (Situacao == SituacaoProposta.Enviada)
        {
            comentario.PropostaId = Id;
            Comentarios.Add(comentario);
        }
        return comentario;
    }

    public void RemoveComentario(Comentario comentario)
    {
        Comentarios.Remove(comentario);
    }

    public bool Aprovar()
    {
        if (Situacao != SituacaoProposta.Enviada) return false;
        Situacao = SituacaoProposta.Aceita;
        AdicionarEvento(new PropostaAprovadaEvent(Id, ValorTotal?.Valor ?? 0, ClienteId, SolicitacaoId));
        return true;
    }

    public void Recusar()
    {
        Situacao = SituacaoProposta.Recusada;
    }

    public void Cancelar()
    {
        Situacao = SituacaoProposta.Cancelada;
    }
}