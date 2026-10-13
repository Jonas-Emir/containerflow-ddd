namespace ContainerFlow.Vendas.Propostas;

public class Comentario
{
    public Comentario()
    {
        Id = Guid.NewGuid();
        Texto = string.Empty;
        Usuario = string.Empty;
        Data = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public string Texto { get; set; }
    public string Usuario { get; set; }
    public DateTime Data { get; set; }
    public Guid PropostaId { get; set; }
    public Proposta? Proposta { get; set; }
}
