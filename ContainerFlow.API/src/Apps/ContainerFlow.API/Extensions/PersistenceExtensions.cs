using ContainerFlow.Api.Eventos;
using ContainerFlow.API.Data;
using ContainerFlow.API.Data.Repositories;
using ContainerFlow.API.Identity;
using ContainerFlow.Clientes.Cadastro;
using ContainerFlow.Contracts;
using ContainerFlow.Engenharia.Containers;
using ContainerFlow.Financeiro.Faturamento;
using ContainerFlow.Vendas.Locacoes;
using ContainerFlow.Vendas.Propostas;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.API.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ClienteRepository>();
        services.AddScoped<IClienteRepository>(sp => sp.GetRequiredService<ClienteRepository>());
        services.AddScoped<IRepository<Cliente>>(sp => sp.GetRequiredService<ClienteRepository>());

        services.AddScoped<SolicitacaoRepository>();
        services.AddScoped<ISolicitacaoRepository>(sp => sp.GetRequiredService<SolicitacaoRepository>());
        services.AddScoped<IRepository<PedidoLocacao>>(sp => sp.GetRequiredService<SolicitacaoRepository>());

        services.AddScoped<PropostaRepository>();
        services.AddScoped<IPropostaRepository>(sp => sp.GetRequiredService<PropostaRepository>());
        services.AddScoped<IRepository<Proposta>>(sp => sp.GetRequiredService<PropostaRepository>());

        services.AddScoped<LocacaoRepository>();
        services.AddScoped<ILocacaoRepository>(sp => sp.GetRequiredService<LocacaoRepository>());
        services.AddScoped<IRepository<Locacao>>(sp => sp.GetRequiredService<LocacaoRepository>());

        services.AddScoped<ConteinerRepository>();
        services.AddScoped<IConteinerRepository>(sp => sp.GetRequiredService<ConteinerRepository>());
        services.AddScoped<IRepository<Conteiner>>(sp => sp.GetRequiredService<ConteinerRepository>());

        services.AddScoped<FaturaRepository>();
        services.AddScoped<IFaturaRepository>(sp => sp.GetRequiredService<FaturaRepository>());
        services.AddScoped<IRepository<Fatura>>(sp => sp.GetRequiredService<FaturaRepository>());

        services.AddScoped<IEventoManager, EventoManager>();
        services.AddScoped<IAcessoManager, AcessoManagerWithIdentity>();

        return services;
    }
}
