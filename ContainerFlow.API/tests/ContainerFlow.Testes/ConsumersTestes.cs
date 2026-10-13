using ContainerFlow.Api.Eventos;
using ContainerFlow.API.Data;
using ContainerFlow.API.Data.Repositories;
using ContainerFlow.Contracts.Eventos;
using ContainerFlow.Engenharia.Containers;
using ContainerFlow.Financeiro.Faturamento;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContainerFlow.Testes;

public class ConsumersTestes
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task EmissorDeFaturas_DeveGerarFatura_AoConsumirPropostaAprovadaEvent()
    {
        using var context = CreateInMemoryDbContext();
        var repoFatura = new FaturaRepository(context);
        var eventoManager = new EventoManager(context);
        var emissor = new EmissorDeFaturas(repoFatura, eventoManager, context);

        var propostaId = Guid.NewGuid();
        var evento = new PropostaAprovadaEvent(propostaId, 4500, Guid.NewGuid(), Guid.NewGuid());
        context.Outbox.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CorrelationId = evento.EventoId,
            TipoEvento = nameof(PropostaAprovadaEvent),
            InfoEvento = System.Text.Json.JsonSerializer.Serialize(evento),
            DataCriacaoUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        await emissor.ExecutarAsync();

        var faturas = await repoFatura.ObterTodasAsync();
        Assert.Single(faturas);
        Assert.Equal(4500, faturas[0].Total);
        Assert.StartsWith("FAT-", faturas[0].Numero);
    }

    [Fact]
    public async Task ReservadorDeConteiner_DeveReservarConteinerDisponivel_AoConsumirPropostaAprovadaEvent()
    {
        using var context = CreateInMemoryDbContext();
        var repoConteiner = new ConteinerRepository(context);
        var eventoManager = new EventoManager(context);
        var reservador = new ReservadorDeConteiner(repoConteiner, eventoManager, context);

        var conteiner = new Conteiner
        {
            Id = Guid.NewGuid(),
            Status = StatusConteiner.OFF
        };
        await repoConteiner.AdicionarAsync(conteiner);

        var propostaId = Guid.NewGuid();
        var evento = new PropostaAprovadaEvent(propostaId, 3000, Guid.NewGuid(), Guid.NewGuid());
        context.Outbox.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CorrelationId = evento.EventoId,
            TipoEvento = nameof(PropostaAprovadaEvent),
            InfoEvento = System.Text.Json.JsonSerializer.Serialize(evento),
            DataCriacaoUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        await reservador.ExecutarAsync();

        var conteinerAtualizado = await repoConteiner.ObterPorIdAsync(conteiner.Id);
        Assert.NotNull(conteinerAtualizado);
        Assert.Equal(propostaId, conteinerAtualizado.LocacaoId);
        Assert.Equal(StatusConteiner.STANDBY, conteinerAtualizado.Status);
    }
}
