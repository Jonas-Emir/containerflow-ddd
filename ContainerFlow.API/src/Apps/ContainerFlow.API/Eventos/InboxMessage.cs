namespace ContainerFlow.Api.Eventos;

public class InboxMessage
{
    public Guid Id { get; set; }
    public required string TipoLeitor { get; set; }
    public Guid OutboxMessageId { get; set; }
    public OutboxMessage? Evento { get; set; }
    public DateTime DataProcessamentoUtc { get; set; }
    public bool Sucesso { get; set; } = true;
    public string? Erro { get; set; }
}
