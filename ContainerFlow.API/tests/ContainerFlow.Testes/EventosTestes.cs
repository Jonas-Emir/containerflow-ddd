using ContainerFlow.Api.Eventos;
using ContainerFlow.API.Data;
using ContainerFlow.Contracts.Eventos;
using ContainerFlow.Vendas.Propostas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContainerFlow.Testes;

public class EventosTestes
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AprovarProposta_DeveRegistrarEventoNoOutbox_AoSalvarNoDbContext()
    {
        using var context = CreateInMemoryDbContext();

        var proposta = new Proposta
        {
            Id = Guid.NewGuid(),
            Situacao = SituacaoProposta.Enviada,
            ValorTotal = new ValorMonetario(5000),
            ClienteId = Guid.NewGuid(),
            SolicitacaoId = Guid.NewGuid(),
            NomeArquivo = "proposta.pdf"
        };

        var aprovada = proposta.Aprovar();
        Assert.True(aprovada);
        Assert.Single(proposta.Eventos);

        context.Propostas.Add(proposta);
        await context.SaveChangesAsync();

        // Após salvar, os eventos devem ter sido convertidos em OutboxMessages e limpos do agregado
        Assert.Empty(proposta.Eventos);

        var outboxMessages = await context.Outbox.ToListAsync();
        Assert.Single(outboxMessages);
        Assert.Equal(nameof(PropostaAprovadaEvent), outboxMessages[0].TipoEvento);
        Assert.Contains("5000", outboxMessages[0].InfoEvento);
    }

    [Fact]
    public async Task EventoManager_DeveProcessarMensagemEGravarInbox_DeFormaIdempotente()
    {
        using var context = CreateInMemoryDbContext();
        var eventoManager = new EventoManager(context);

        var propostaId = Guid.NewGuid();
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            TipoEvento = nameof(PropostaAprovadaEvent),
            InfoEvento = System.Text.Json.JsonSerializer.Serialize(new PropostaAprovadaEvent(propostaId, 2500, Guid.NewGuid(), Guid.NewGuid())),
            DataCriacaoUtc = DateTime.UtcNow
        };

        context.Outbox.Add(outboxMessage);
        await context.SaveChangesAsync();

        var execucoes = 0;
        await eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            nameof(PropostaAprovadaEvent),
            "TestReader",
            (msg, ct) =>
            {
                execucoes++;
                Assert.Equal(propostaId, msg.Corpo.IdProposta);
                Assert.Equal(2500, msg.Corpo.ValorProposta);
                return Task.CompletedTask;
            });

        Assert.Equal(1, execucoes);

        // Validar que Inbox foi gravado
        var inbox = await context.Inbox.FirstOrDefaultAsync(i => i.OutboxMessageId == outboxMessage.Id && i.TipoLeitor == "TestReader");
        Assert.NotNull(inbox);
        Assert.True(inbox.Sucesso);

        // Segunda execução: Não deve reprocessar pois já foi marcado como sucesso
        await eventoManager.ProcessarEventoIdempotenteAsync<PropostaAprovadaEvent>(
            nameof(PropostaAprovadaEvent),
            "TestReader",
            (msg, ct) =>
            {
                execucoes++;
                return Task.CompletedTask;
            });

        Assert.Equal(1, execucoes);
    }
}
