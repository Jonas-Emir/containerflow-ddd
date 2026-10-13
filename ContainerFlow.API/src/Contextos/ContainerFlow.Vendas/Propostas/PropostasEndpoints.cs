using ContainerFlow.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Vendas.Propostas;

public static class PropostasEndpoints
{
    public const string ENDPOINT_NAME_GET_PROPOSTA = "GetProposta";

    public static IEndpointRouteBuilder MapPropostasEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup(EndpointConstants.ROUTE_PEDIDOS)
            .WithTags(EndpointConstants.TAG_LOCACAO)
            .WithOpenApi();

        group
            .MapPostProposta()
            .MapGetPropostas()
            .MapGetPropostaById()
            .MapPatchAcceptProposta()
            .MapPatchRejectProposta()
            .MapPostComentarioProposta();

        return builder;
    }

    public static RouteGroupBuilder MapPostProposta(this RouteGroupBuilder builder)
    {
        builder
            .MapPost("{id:guid}/proposals", async (
                [FromRoute] Guid id,
                [FromForm] PropostaRequest request,
                [FromServices] ISolicitacaoRepository repoSolicitacao,
                [FromServices] IPropostaRepository repoProposta,
                [FromServices] IUnitOfWork unitOfWork,
                CancellationToken cancellationToken
                ) =>
            {
                var solicitacao = await repoSolicitacao.ObterPorIdAsync(id, cancellationToken);
                if (solicitacao is null) return Results.NotFound();

                var proposta = new Proposta
                {
                    Id = Guid.NewGuid(),
                    ClienteId = solicitacao.ClienteId,
                    ValorTotal = new ValorMonetario(request.ValorTotal),
                    DataCriacao = DateTime.UtcNow,
                    DataExpiracao = request.DataExpiracao,
                    NomeArquivo = request.Arquivo.FileName,
                    SolicitacaoId = solicitacao.Id
                };

                await repoProposta.AdicionarAsync(proposta, cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Results.CreatedAtRoute(
                    ENDPOINT_NAME_GET_PROPOSTA,
                    new { Id = proposta.SolicitacaoId, PropostaId = proposta.Id },
                    PropostaResponse.From(proposta));
            })
            .RequireAuthorization(policy => policy.RequireRole("Suporte"))
            .DisableAntiforgery()
            .WithSummary("Vendedor envia proposta de locação")
            .Produces(StatusCodes.Status404NotFound)
            .Produces<PropostaResponse>(StatusCodes.Status201Created);
        return builder;
    }

    public static RouteGroupBuilder MapGetPropostaById(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}/proposals/{propostaId:guid}", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IHttpContextAccessor accessor,
            [FromServices] IPropostaRepository repository,
            CancellationToken cancellationToken) =>
        {
            var clienteId = accessor.HttpContext?.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (clienteId is null) return Results.Unauthorized();

            var proposta = await repository.ObterPorIdEPedidoAsync(propostaId, id, cancellationToken);
            if (proposta is null || proposta.ClienteId != Guid.Parse(clienteId)) return Results.NotFound();

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithName(ENDPOINT_NAME_GET_PROPOSTA)
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .WithSummary("Cliente consulta detalhes de uma proposta de locação")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapGetPropostas(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}/proposals", async (
            [FromRoute] Guid id,
            [FromServices] IHttpContextAccessor accessor,
            [FromServices] ISolicitacaoRepository repository,
            CancellationToken cancellationToken) =>
        {
            var clienteId = accessor.HttpContext?.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (clienteId is null) return Results.Unauthorized();

            var solicitacao = await repository.ObterPorIdAsync(id, cancellationToken);
            if (solicitacao is null || solicitacao.ClienteId != Guid.Parse(clienteId)) return Results.NotFound();

            return Results.Ok(solicitacao.Propostas.Select(p => PropostaResponse.From(p)));
        })
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .WithSummary("Cliente consulta as propostas para uma solicitação de locação")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<IEnumerable<PropostaResponse>>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPatchAcceptProposta(this RouteGroupBuilder builder)
    {
        builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/accept", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IPropostaService service,
            CancellationToken cancellationToken) =>
        {
            var casoUso = new AprovarProposta(id, propostaId);
            var proposta = await service.AprovarAsync(casoUso, cancellationToken);
            if (proposta is null) return Results.NotFound();
            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Cliente aceita proposta de locação.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPatchRejectProposta(this RouteGroupBuilder builder)
    {
        builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/reject", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IPropostaRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var proposta = await repository.ObterPorIdEPedidoAsync(propostaId, id, cancellationToken);
            if (proposta is null) return Results.NotFound();

            proposta.Recusar();
            await repository.AtualizarAsync(proposta, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Cliente rejeita proposta de locação.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPostComentarioProposta(this RouteGroupBuilder builder)
    {
        builder.MapPost("{id:guid}/proposals/{propostaId:guid}/comment", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromBody] ComentarioRequest request,
            HttpContext context,
            [FromServices] IPropostaRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var proposta = await repository.ObterPorIdEPedidoAsync(propostaId, id, cancellationToken);
            if (proposta is null) return Results.NotFound();

            string? quem = context.User.Identity?.Name;
            if (quem is null) return Results.Unauthorized();

            proposta.AddComentario(new Comentario
            {
                Id = Guid.NewGuid(),
                Data = DateTime.UtcNow,
                Usuario = quem,
                Texto = request.Comentario
            });

            await repository.AtualizarAsync(proposta, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Vendedor/Cliente comenta proposta.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente", "Suporte"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }
}
