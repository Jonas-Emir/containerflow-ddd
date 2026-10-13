using System.Text.Json;
using ContainerFlow.API.Data;
using ContainerFlow.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.Api.Eventos;

public class EventoManager : IEventoManager
{
    private readonly AppDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EventoManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task MarcarComoLidaAsync(
        Guid idEvento,
        string tipoLeitor,
        bool sucesso = true,
        string? erro = null,
        CancellationToken cancellationToken = default)
    {
        var outbox = await _context.Outbox
            .FirstOrDefaultAsync(o => o.Id == idEvento, cancellationToken);
        if (outbox is null) return;

        var inboxExistente = await _context.Inbox
            .FirstOrDefaultAsync(i => i.OutboxMessageId == outbox.Id && i.TipoLeitor == tipoLeitor, cancellationToken);

        if (inboxExistente is not null)
        {
            inboxExistente.Sucesso = sucesso;
            inboxExistente.Erro = erro;
            inboxExistente.DataProcessamentoUtc = DateTime.UtcNow;
        }
        else
        {
            var inbox = new InboxMessage
            {
                Id = Guid.NewGuid(),
                OutboxMessageId = outbox.Id,
                TipoLeitor = tipoLeitor,
                DataProcessamentoUtc = DateTime.UtcNow,
                Sucesso = sucesso,
                Erro = erro
            };
            await _context.Inbox.AddAsync(inbox, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Mensagem<T>>> RecuperarNaoLidasAsync<T>(
        string tipoEvento,
        string tipoLeitor,
        CancellationToken cancellationToken = default)
    {
        var mensagens = await _context.Outbox
            .Where(o => o.TipoEvento == tipoEvento &&
                        !_context.Inbox.Any(i => i.OutboxMessageId == o.Id && i.TipoLeitor == tipoLeitor && i.Sucesso))
            .OrderBy(o => o.DataCriacaoUtc)
            .ToListAsync(cancellationToken);

        var saida = new List<Mensagem<T>>();

        foreach (var outbox in mensagens)
        {
            var obj = JsonSerializer.Deserialize<T>(outbox.InfoEvento, JsonOptions);
            if (obj is not null)
            {
                saida.Add(new Mensagem<T>
                {
                    Id = outbox.Id,
                    CorrelationId = outbox.CorrelationId,
                    OcorreuEmUtc = outbox.DataCriacaoUtc,
                    Corpo = obj
                });
            }
        }

        return saida.AsReadOnly();
    }

    public async Task ProcessarEventoIdempotenteAsync<T>(
        string tipoEvento,
        string tipoLeitor,
        Func<Mensagem<T>, CancellationToken, Task> manipulador,
        CancellationToken cancellationToken = default)
    {
        var mensagens = await RecuperarNaoLidasAsync<T>(tipoEvento, tipoLeitor, cancellationToken);

        foreach (var mensagem in mensagens)
        {
            try
            {
                await manipulador(mensagem, cancellationToken);
                await MarcarComoLidaAsync(mensagem.Id, tipoLeitor, sucesso: true, erro: null, cancellationToken);
            }
            catch (Exception ex)
            {
                await MarcarComoLidaAsync(mensagem.Id, tipoLeitor, sucesso: false, erro: ex.Message, cancellationToken);
                throw;
            }
        }
    }
}
