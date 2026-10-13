namespace ContainerFlow.Api.Eventos;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public required string TipoEvento { get; set; }
    public required string InfoEvento { get; set; }
    public DateTime DataCriacaoUtc { get; set; }
    public int Tentativas { get; set; } = 0;
    public string? UltimoErro { get; set; }
}
