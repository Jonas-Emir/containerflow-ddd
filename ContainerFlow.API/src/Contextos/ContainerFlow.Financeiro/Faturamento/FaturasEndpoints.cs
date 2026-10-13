using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ContainerFlow.Financeiro.Faturamento;

public record FaturaResponse(Guid Id, string Numero, decimal Total, Guid LocacaoId, string Status, DateTime DataEmissao, DateTime DataVencimento)
{
    public static FaturaResponse From(Fatura fatura) =>
        new(fatura.Id, fatura.Numero, fatura.Total, fatura.LocacaoId, fatura.Status.ToString(), fatura.DataEmissao, fatura.DataVencimento);
}

public static class FaturasEndpoints
{
    public const string ENDPOINT_NAME_GET_FATURA = "GetFatura";

    public static IEndpointRouteBuilder MapFaturasEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("api/faturas")
            .RequireAuthorization(policy => policy.RequireRole("Cliente", "Suporte"))
            .WithTags("Financeiro")
            .WithOpenApi();

        group
            .MapGetFaturaById()
            .MapGetFaturasPorLocacao();

        return builder;
    }

    public static RouteGroupBuilder MapGetFaturaById(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IFaturaRepository repository,
            CancellationToken cancellationToken) =>
        {
            var fatura = await repository.ObterPorIdAsync(id, cancellationToken);
            if (fatura is null) return Results.NotFound();

            return Results.Ok(FaturaResponse.From(fatura));
        })
        .WithName(ENDPOINT_NAME_GET_FATURA)
        .WithSummary("Consulta uma fatura pelo identificador")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<FaturaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapGetFaturasPorLocacao(this RouteGroupBuilder builder)
    {
        builder.MapGet("locacao/{locacaoId:guid}", async (
            [FromRoute] Guid locacaoId,
            [FromServices] IFaturaRepository repository,
            CancellationToken cancellationToken) =>
        {
            var faturas = await repository.ObterPorLocacaoAsync(locacaoId, cancellationToken);
            return Results.Ok(faturas.Select(FaturaResponse.From));
        })
        .WithSummary("Lista as faturas emitidas para uma locação")
        .Produces<IEnumerable<FaturaResponse>>(StatusCodes.Status200OK);

        return builder;
    }
}
