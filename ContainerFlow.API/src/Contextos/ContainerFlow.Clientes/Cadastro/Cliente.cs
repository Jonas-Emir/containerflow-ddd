using ContainerFlow.DDD;

namespace ContainerFlow.Clientes.Cadastro;

public class Cliente : AggregateRoot<Guid>
{
    private Cliente()
    {
        Nome = string.Empty;
        CPF = string.Empty;
        Email = null!;
    }

    public Cliente(string nome, Email email, string cPF)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        CPF = cPF;
    }

    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string CPF { get; private set; }
    public string? Celular { get; set; }
    public ICollection<Endereco> Enderecos { get; private set; } = new List<Endereco>();

    public Endereco AddEndereco(Endereco endereco)
    {
        endereco.ClienteId = Id;
        Enderecos.Add(endereco);
        return endereco;
    }

    public void RemoveEndereco(Endereco endereco)
    {
        Enderecos.Remove(endereco);
    }

    public void AtualizarCelular(string? celular)
    {
        Celular = celular;
    }

    public Endereco AddEndereco(string cep, string rua, string? numero, string? complemento, string? bairro, string municipio, UnidadeFederativa? estado)
    {
        var endereco = new Endereco
        {
            Id = Guid.NewGuid(),
            CEP = cep,
            Rua = rua,
            Numero = numero,
            Complemento = complemento,
            Bairro = bairro,
            Municipio = municipio,
            Estado = estado
        };
        return AddEndereco(endereco);
    }
}
