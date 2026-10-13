using ContainerFlow.Engenharia.Containers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.Engenharia;

public static class EngenhariaModuleExtensions
{
    public static IServiceCollection AddEngenhariaModule(this IServiceCollection services)
    {
        services.AddScoped<ReservadorDeConteiner>();
        return services;
    }

    public static IEndpointRouteBuilder MapEngenhariaModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapConteineresEndpoints();
        return endpoints;
    }
}
