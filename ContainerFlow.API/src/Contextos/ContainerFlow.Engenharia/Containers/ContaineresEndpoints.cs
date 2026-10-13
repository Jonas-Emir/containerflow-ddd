using ContainerFlow.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Engenharia.Containers;

public static class ContaineresEndpoints
{
    public const string ENDPOINT_NAME_GET_CONTEINER = "GetConteiner";

    public static IEndpointRouteBuilder MapConteineresEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup(EndpointConstants.ROUTE_CONTEINERES)
            .RequireAuthorization(builder => builder.RequireRole("Cliente"))
            .WithTags(EndpointConstants.TAG_CONTEINERES)
            .WithOpenApi();

        group
            .MapGetConteinerById();

        return builder;
    }

    public static RouteGroupBuilder MapGetConteinerById(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IConteinerRepository repository,
            CancellationToken cancellationToken) =>
        {
            var conteiner = await repository.ObterPorIdAsync(id, cancellationToken);
            if (conteiner is null) return Results.NotFound();

            return Results.Ok(ConteinerResponse.From(conteiner));
        })
        .WithName(ENDPOINT_NAME_GET_CONTEINER)
        .WithSummary("Cliente consulta informações sobre o contêiner")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ConteinerResponse>(StatusCodes.Status200OK);

        return builder;
    }
}
