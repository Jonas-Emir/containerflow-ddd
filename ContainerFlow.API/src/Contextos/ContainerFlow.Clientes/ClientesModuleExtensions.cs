using ContainerFlow.Clientes.Cadastro;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.Clientes;

public static class ClientesModuleExtensions
{
    public static IServiceCollection AddClientesModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapClientesModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapClientesEndpoints();
        endpoints.MapAprovacaoClientesEndpoints();
        return endpoints;
    }
}
