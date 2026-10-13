using ContainerFlow.DDD;

namespace ContainerFlow.Engenharia.Containers;

/// <summary>
///         ON - O contêiner está ligado e funcionando normalmente.
///         OFF - O contêiner está desligado.
///        STANDBY - O contêiner está em modo de espera, com consumo reduzido de energia.
///        LOW_POWER - O contêiner está operando em um modo de baixa energia para economizar recursos.
///        FAULT - Há uma falha no sistema de energia do contêiner.
///        CHARGING - O contêiner está conectado a uma fonte de energia e sendo carregado.
/// </summary>
public enum StatusConteiner
{
    ON,
    OFF,
    STANDBY,
    LOW_POWER,
    FAULT,
    CHARGING
}

public class Conteiner : AggregateRoot<Guid>
{
    public Conteiner()
    {
        Id = Guid.NewGuid();
    }

    public StatusConteiner Status { get; set; } = StatusConteiner.OFF;
    public string? Observacoes { get; set; }
    public Guid LocacaoId { get; set; }

    public void ReservarParaLocacao(Guid locacaoId)
    {
        LocacaoId = locacaoId;
        Status = StatusConteiner.STANDBY;
        Observacoes = $"Reservado para locacao/proposta {locacaoId} em {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
    }

    public void LiberarLocacao()
    {
        LocacaoId = Guid.Empty;
        Status = StatusConteiner.OFF;
        Observacoes = $"Liberado em {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
    }

    public void AlterarStatus(StatusConteiner novoStatus, string? motivo = null)
    {
        Status = novoStatus;
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            Observacoes = motivo;
        }
    }
}
