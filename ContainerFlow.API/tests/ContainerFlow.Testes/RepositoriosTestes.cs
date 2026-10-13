using ContainerFlow.API.Data;
using ContainerFlow.API.Data.Repositories;
using ContainerFlow.Contracts;
using ContainerFlow.Vendas.Locacoes;
using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContainerFlow.Testes;

public class RepositoriosTestes
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task PropostaService_AprovarAsync_DeveAprovarPropostaECriarLocacaoComCommitAtomico()
    {
        using var context = CreateInMemoryDbContext();
        var repoProposta = new PropostaRepository(context);
        var repoLocacao = new LocacaoRepository(context);
        var repoSolicitacao = new SolicitacaoRepository(context);
        var calculadora = new CalculadoraPadraoPrazosLocacao();
        var service = new PropostaService(repoProposta, repoLocacao, context, calculadora);

        var clienteId = Guid.NewGuid();
        var pedido = new PedidoLocacao
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Descricao = "Locacao de 2 conteineres",
            QuantidadeEstimada = 2,
            Finalidade = "Obras",
            DataInicioOperacao = DateTime.UtcNow.AddDays(10),
            DisponibilidadePrevia = 2,
            DuracaoPrevistaLocacao = 30
        };
        await repoSolicitacao.AdicionarAsync(pedido);

        var proposta = new Proposta
        {
            Id = Guid.NewGuid(),
            Situacao = SituacaoProposta.Enviada,
            ValorTotal = new ValorMonetario(7500),
            ClienteId = clienteId,
            SolicitacaoId = pedido.Id,
            Solicitacao = pedido,
            NomeArquivo = "proposta-teste.pdf"
        };

        await repoProposta.AdicionarAsync(proposta);
        await context.CommitAsync();

        var resultado = await service.AprovarAsync(new AprovarProposta(pedido.Id, proposta.Id));

        Assert.NotNull(resultado);
        Assert.Equal(SituacaoProposta.Aceita, resultado.Situacao);

        // Validar que locação foi criada e persistida no mesmo commit
        var locacoes = await repoLocacao.ObterPorClienteAsync(clienteId);
        Assert.Single(locacoes);
        Assert.Equal(proposta.Id, locacoes[0].PropostaId);
        Assert.Equal(clienteId, locacoes[0].ClienteId);
    }

    [Fact]
    public async Task ClienteRepository_ObterPorEmailAsync_DeveRetornarClienteComEnderecos()
    {
        using var context = CreateInMemoryDbContext();
        var repo = new ClienteRepository(context);

        var cliente = new ContainerFlow.Clientes.Cadastro.Cliente("João Silva", new ContainerFlow.Clientes.Cadastro.Email("joao@teste.com"), "12345678901")
        {
            Celular = "11999999999"
        };
        cliente.AddEndereco(new ContainerFlow.Clientes.Cadastro.Endereco
        {
            Id = Guid.NewGuid(),
            CEP = "01001-000",
            Rua = "Praça da Sé",
            Numero = "100",
            Bairro = "Centro",
            Municipio = "São Paulo"
        });

        await repo.AdicionarAsync(cliente);
        await context.CommitAsync();

        var clienteSalvo = await repo.ObterPorEmailAsync("joao@teste.com");

        Assert.NotNull(clienteSalvo);
        Assert.Equal("João Silva", clienteSalvo.Nome);
        Assert.Single(clienteSalvo.Enderecos);
        Assert.Equal("01001-000", clienteSalvo.Enderecos.First().CEP);
    }
}
