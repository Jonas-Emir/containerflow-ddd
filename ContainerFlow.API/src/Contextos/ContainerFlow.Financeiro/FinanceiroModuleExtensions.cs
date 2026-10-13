using ContainerFlow.Financeiro.Faturamento;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.Financeiro;

public static class FinanceiroModuleExtensions
{
    public static IServiceCollection AddFinanceiroModule(this IServiceCollection services)
    {
        services.AddScoped<EmissorDeFaturas>();
        return services;
    }

    public static IEndpointRouteBuilder MapFinanceiroModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapFaturasEndpoints();
        return endpoints;
    }
}
