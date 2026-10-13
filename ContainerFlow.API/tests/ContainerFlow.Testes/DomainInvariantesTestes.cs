using ContainerFlow.Contracts.Eventos;
using ContainerFlow.DDD;
using ContainerFlow.Engenharia.Containers;
using ContainerFlow.Financeiro.Faturamento;
using ContainerFlow.Vendas.Locacoes;
using ContainerFlow.Vendas.Propostas;

namespace ContainerFlow.Testes;

public class DomainInvariantesTestes
{
    [Fact]
    public void Proposta_AoAprovar_DeveMudarSituacaoParaAceitaEPublicarEvento()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var solicitacaoId = Guid.NewGuid();
        var proposta = new Proposta
        {
            ClienteId = clienteId,
            SolicitacaoId = solicitacaoId,
            ValorTotal = new ValorMonetario(5000m),
            Situacao = SituacaoProposta.Enviada
        };

        // Act
        var aprovado = proposta.Aprovar();

        // Assert
        Assert.True(aprovado);
        Assert.Equal(SituacaoProposta.Aceita, proposta.Situacao);
        Assert.Single(proposta.Eventos);
        var evento = Assert.IsType<PropostaAprovadaEvent>(proposta.Eventos.First());
        Assert.Equal(proposta.Id, evento.IdProposta);
        Assert.Equal(5000m, evento.ValorProposta);
        Assert.Equal(clienteId, evento.ClienteId);
        Assert.Equal(solicitacaoId, evento.SolicitacaoId);
    }

    [Fact]
    public void Proposta_AoTentarAprovarPropostaNaoEnviada_DeveRetornarFalso()
    {
        // Arrange
        var proposta = new Proposta
        {
            Situacao = SituacaoProposta.Aceita,
            ValorTotal = new ValorMonetario(5000m)
        };

        // Act
        var aprovado = proposta.Aprovar();

        // Assert
        Assert.False(aprovado);
        Assert.Empty(proposta.Eventos);
    }

    [Fact]
    public void Proposta_RecusarECancelar_DeveAtualizarSituacoesCorretamente()
    {
        // Arrange
        var proposta1 = new Proposta();
        var proposta2 = new Proposta();

        // Act
        proposta1.Recusar();
        proposta2.Cancelar();

        // Assert
        Assert.Equal(SituacaoProposta.Recusada, proposta1.Situacao);
        Assert.Equal(SituacaoProposta.Cancelada, proposta2.Situacao);
    }

    [Fact]
    public void Conteiner_AoReservarParaLocacao_DeveAtualizarStatusParaStandbyERegistrarLocacao()
    {
        // Arrange
        var conteiner = new Conteiner();
        var locacaoId = Guid.NewGuid();

        // Act
        conteiner.ReservarParaLocacao(locacaoId);

        // Assert
        Assert.Equal(StatusConteiner.STANDBY, conteiner.Status);
        Assert.Equal(locacaoId, conteiner.LocacaoId);
        Assert.Contains(locacaoId.ToString(), conteiner.Observacoes);
    }

    [Fact]
    public void Conteiner_AoLiberarLocacao_DeveVoltarParaOffELimparLocacao()
    {
        // Arrange
        var conteiner = new Conteiner();
        conteiner.ReservarParaLocacao(Guid.NewGuid());

        // Act
        conteiner.LiberarLocacao();

        // Assert
        Assert.Equal(StatusConteiner.OFF, conteiner.Status);
        Assert.Equal(Guid.Empty, conteiner.LocacaoId);
        Assert.Contains("Liberado", conteiner.Observacoes);
    }

    [Fact]
    public void Fatura_AoMarcarComoPaga_DeveAtualizarStatusParaPaga()
    {
        // Arrange
        var fatura = new Fatura("FAT-2026-001", 1500m, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(30));

        // Act
        fatura.MarcarComoPaga();

        // Assert
        Assert.Equal(StatusFatura.Paga, fatura.Status);
    }

    [Fact]
    public void Fatura_AoTentarPagarFaturaCancelada_DeveLancarDomainException()
    {
        // Arrange
        var fatura = new Fatura("FAT-2026-002", 1500m, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        fatura.Cancelar();

        // Act & Assert
        Assert.Throws<DomainException>(() => fatura.MarcarComoPaga());
    }

    [Fact]
    public void Fatura_AoTentarCancelarFaturaPaga_DeveLancarDomainException()
    {
        // Arrange
        var fatura = new Fatura("FAT-2026-003", 1500m, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        fatura.MarcarComoPaga();

        // Act & Assert
        Assert.Throws<DomainException>(() => fatura.Cancelar());
    }

    [Fact]
    public void CalculadoraPadraoPrazosLocacao_DeveCalcularPrazosDeEntregaETermino()
    {
        // Arrange
        var calculadora = new CalculadoraPadraoPrazosLocacao();
        var dataInicio = new DateTime(2026, 10, 20);
        var pedido = new PedidoLocacao
        {
            DataInicioOperacao = dataInicio,
            DisponibilidadePrevia = 3,
            DuracaoPrevistaLocacao = 30
        };

        var proposta = new Proposta
        {
            Solicitacao = pedido
        };

        // Act
        var entrega = calculadora.CalculaDataPrevistaParaEntrega(proposta);
        var termino = calculadora.CalculaDataPrevistaParaTermino(proposta);

        // Assert
        Assert.Equal(new DateTime(2026, 10, 17), entrega);
        Assert.Equal(new DateTime(2026, 11, 19), termino);
    }

    [Fact]
    public void CalculadoraPadraoPrazosLocacao_QuandoSolicitacaoNula_DeveLancarInvalidOperationException()
    {
        // Arrange
        var calculadora = new CalculadoraPadraoPrazosLocacao();
        var proposta = new Proposta { Solicitacao = null };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => calculadora.CalculaDataPrevistaParaEntrega(proposta));
        Assert.Throws<InvalidOperationException>(() => calculadora.CalculaDataPrevistaParaTermino(proposta));
    }
}
