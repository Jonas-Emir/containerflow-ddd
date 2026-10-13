using ContainerFlow.Vendas.Locacoes;
using ContainerFlow.Vendas.Propostas;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.Vendas;

public static class VendasModuleExtensions
{
    public static IServiceCollection AddVendasModule(this IServiceCollection services)
    {
        services.AddScoped<ICalculadoraPrazosLocacao, CalculadoraPadraoPrazosLocacao>();
        services.AddScoped<IPropostaService, PropostaService>();
        return services;
    }

    public static IEndpointRouteBuilder MapVendasModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapSolicitacoesEndpoints();
        endpoints.MapPropostasEndpoints();
        endpoints.MapLocacoesEndpoints();
        return endpoints;
    }
}
