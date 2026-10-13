using ContainerFlow.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Vendas.Locacoes;

public static class LocacoesEndpoints
{
    public static IEndpointRouteBuilder MapLocacoesEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup(EndpointConstants.ROUTE_LOCACOES)
            .RequireAuthorization(builder => builder.RequireRole("Cliente"))
            .WithTags(EndpointConstants.TAG_LOCACAO)
            .WithOpenApi();

        group
            .MapGetLocacoes();

        return builder;
    }

    public static RouteGroupBuilder MapGetLocacoes(this RouteGroupBuilder builder)
    {
        builder.MapGet("", async (
            HttpContext context,
            [FromServices] ILocacaoRepository repository,
            CancellationToken cancellationToken) =>
        {
            var clienteId = context.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();
            if (clienteId is null) return Results.Unauthorized();

            var locacoes = await repository.ObterPorClienteAsync(Guid.Parse(clienteId), cancellationToken);
            return Results.Ok(locacoes.Select(LocacaoResponse.From));
        })
        .WithSummary("Lista o histórico de locações do cliente");

        return builder;
    }
}
