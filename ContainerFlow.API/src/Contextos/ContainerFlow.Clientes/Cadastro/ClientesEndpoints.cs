using ContainerFlow.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Clientes.Cadastro;

public static class ClientesEndpoints
{
    public const string ENDPOINT_NAME_GET_CLIENTE = "GetCliente";

    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup(EndpointConstants.ROUTE_CLIENTES)
            .RequireAuthorization(policy => policy.RequireRole("Cliente"))
            .WithTags(EndpointConstants.TAG_CLIENTES)
            .WithOpenApi();

        group
            .MapGetClienteById()
            .MapPostClientes()
            .MapPutCliente()
            .MapDeleteCliente()
            .MapGetRegistrationStatus()
            .MapPostEndereco()
            .MapPutEndereco()
            .MapDeleteEndereco();

        return builder;
    }

    public static RouteGroupBuilder MapGetClienteById(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id}", async (
            [FromRoute] Guid id,
            [FromServices] IClienteRepository repository,
            CancellationToken cancellationToken) =>
        {
            var cliente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (cliente is null) return Results.NotFound();

            return Results.Ok(ClienteResponse.From(cliente));
        })
        .WithName(ENDPOINT_NAME_GET_CLIENTE)
        .Produces<ClienteResponse>(StatusCodes.Status200OK);
        return builder;
    }

    public static RouteGroupBuilder MapPostClientes(this RouteGroupBuilder builder)
    {
        builder.MapPost("registration", async (
            [FromBody] RegistroRequest request,
            [FromServices] IClienteRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var clienteExistente = await repository.ObterPorEmailAsync(request.Email, cancellationToken);
            if (clienteExistente is not null) return Results.Conflict("Já existe cliente com o email informado!");

            var cliente = new Cliente(request.Nome, new Email(request.Email), request.CPF)
            {
                Celular = request.Celular
            };
            if (request.Endereco is not null)
            {
                cliente.AddEndereco(request.Endereco.ToModel());
            }
            await repository.AdicionarAsync(cliente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.CreatedAtRoute(ENDPOINT_NAME_GET_CLIENTE, new { id = cliente.Id }, ClienteResponse.From(cliente));
        })
        .AllowAnonymous()
        .Produces<ClienteResponse>(StatusCodes.Status201Created);
        return builder;
    }

    public static RouteGroupBuilder MapPutCliente(this RouteGroupBuilder builder)
    {
        builder.MapPut("{id}", async (
            [FromRoute] Guid id,
            [FromBody] RegistroRequest request,
            [FromServices] IClienteRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var clienteExistente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (clienteExistente is null) return Results.NotFound();

            clienteExistente.Celular = request.Celular;

            await repository.AtualizarAsync(clienteExistente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.Ok(ClienteResponse.From(clienteExistente));
        })
        .Produces<ClienteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        return builder;
    }

    public static RouteGroupBuilder MapDeleteCliente(this RouteGroupBuilder builder)
    {
        builder.MapDelete("{id}", async (
            [FromRoute] Guid id,
            [FromServices] IClienteRepository repository,
            [FromServices] IAcessoManager userManager,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var clienteExistente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (clienteExistente is null) return Results.NotFound();

            await userManager.RemoverClienteAsync(clienteExistente.Email.Value, cancellationToken);
            await repository.RemoverAsync(clienteExistente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
        return builder;
    }

    public static RouteGroupBuilder MapGetRegistrationStatus(this RouteGroupBuilder builder)
    {
        builder.MapGet("registration/status",
            async (
                [FromQuery] string email,
                [FromServices] IClienteRepository repository,
                [FromServices] IAcessoManager userManager,
                CancellationToken cancellationToken) =>
            {
                var cliente = await repository.ObterPorEmailAsync(email, cancellationToken);
                if (cliente is null) return Results.NotFound();

                var acesso = await userManager.ClientePossuiAcessoAsync(cliente.Email.Value, cancellationToken);

                if (!acesso.HasValue)
                    return Results.Ok(RegistrationStatusResponse.Pendente(cliente));

                if (acesso.HasValue && !acesso.Value) return Results.Ok(RegistrationStatusResponse.Reprovado(cliente));

                return Results.Ok(RegistrationStatusResponse.Aprovado(cliente));
            })
            .AllowAnonymous()
            .Produces<RegistrationStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        return builder;
    }

    public static RouteGroupBuilder MapPostEndereco(this RouteGroupBuilder builder)
    {
        builder.MapPost("{id}/enderecos", async (
            [FromRoute] Guid id,
            [FromBody] EnderecoRequest request,
            [FromServices] IClienteRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var cliente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (cliente is null) return Results.NotFound();

            cliente.AddEndereco(request.ToModel());
            await repository.AtualizarAsync(cliente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.CreatedAtRoute(ENDPOINT_NAME_GET_CLIENTE, new { id = cliente.Id }, ClienteResponse.From(cliente));
        })
        .Produces<ClienteResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound);
        return builder;
    }

    public static RouteGroupBuilder MapPutEndereco(this RouteGroupBuilder builder)
    {
        builder.MapPut("{id:guid}/enderecos/{idEndereco:guid}", async (
            [FromRoute] Guid id,
            [FromRoute] Guid idEndereco,
            [FromBody] EnderecoRequest request,
            [FromServices] IClienteRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var cliente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (cliente is null) return Results.NotFound();

            var endereco = cliente.Enderecos.FirstOrDefault(e => e.Id == idEndereco);
            if (endereco is null) return Results.NotFound();

            endereco.CEP = request.CEP ?? endereco.CEP;
            endereco.Rua = request.Rua ?? endereco.Rua;
            endereco.Numero = request.Numero ?? endereco.Numero;
            endereco.Complemento = request.Complemento ?? endereco.Complemento;
            endereco.Bairro = request.Bairro ?? endereco.Bairro;
            endereco.Municipio = request.Municipio ?? endereco.Municipio;
            if (request.Estado is not null)
                endereco.Estado = UfStringConverter.From(request.Estado);

            await repository.AtualizarAsync(cliente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.Ok(ClienteResponse.From(cliente));
        })
        .Produces<ClienteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        return builder;
    }

    public static RouteGroupBuilder MapDeleteEndereco(this RouteGroupBuilder builder)
    {
        builder.MapDelete("{id:guid}/enderecos/{idEndereco:guid}", async (
            [FromRoute] Guid id,
            [FromRoute] Guid idEndereco,
            [FromServices] IClienteRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var cliente = await repository.ObterPorIdAsync(id, cancellationToken);
            if (cliente is null) return Results.NotFound();

            var endereco = cliente.Enderecos.FirstOrDefault(e => e.Id == idEndereco);
            if (endereco is null) return Results.NotFound();

            cliente.RemoveEndereco(endereco);
            await repository.AtualizarAsync(cliente, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
        return builder;
    }
}
