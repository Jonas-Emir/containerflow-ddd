namespace ContainerFlow.Contracts;

public class Mensagem<T>
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime OcorreuEmUtc { get; set; }
    public required T Corpo { get; set; }
}

public interface IEventoManager
{
    Task<IReadOnlyList<Mensagem<T>>> RecuperarNaoLidasAsync<T>(string tipoEvento, string tipoLeitor, CancellationToken cancellationToken = default);
    Task MarcarComoLidaAsync(Guid idEvento, string tipoLeitor, bool sucesso = true, string? erro = null, CancellationToken cancellationToken = default);
    Task ProcessarEventoIdempotenteAsync<T>(
        string tipoEvento,
        string tipoLeitor,
        Func<Mensagem<T>, CancellationToken, Task> manipulador,
        CancellationToken cancellationToken = default);
}