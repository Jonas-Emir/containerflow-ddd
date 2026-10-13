namespace ContainerFlow.Clientes.Cadastro;

public class Endereco
{
    public Endereco() { }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Nome { get; set; }
    public string CEP { get; set; } = string.Empty;
    public string Rua { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Municipio { get; set; } = string.Empty;
    public UnidadeFederativa? Estado { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
}
