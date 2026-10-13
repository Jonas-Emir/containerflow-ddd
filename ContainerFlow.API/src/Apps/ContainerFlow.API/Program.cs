using ContainerFlow.API.Data;
using ContainerFlow.API.Extensions;
using ContainerFlow.API.Identity;
using ContainerFlow.Clientes;
using ContainerFlow.Engenharia;
using ContainerFlow.Engenharia.Containers;
using ContainerFlow.Financeiro;
using ContainerFlow.Financeiro.Faturamento;
using ContainerFlow.Vendas;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDB"));
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ContainerFlowDB"));
});

// Infraestrutura de Persistencia e Repositorios
builder.Services.AddPersistenceInfrastructure();

// Modulos dos Bounded Contexts
builder.Services
    .AddClientesModule()
    .AddVendasModule()
    .AddEngenhariaModule()
    .AddFinanceiroModule();

builder.Services
    .AddIdentityApiEndpoints<AppUser>(options => options.SignIn.RequireConfirmedEmail = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddAuthorization();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = "ClienteId";
});

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("ContainerFlowDB"));
}).AddHangfireServer(options =>
    options.SchedulePollingInterval = TimeSpan.FromSeconds(5));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();

// Endpoints Identity & Modulos
app.MapIdentityEndpoints();
app.MapClientesModule();
app.MapVendasModule();
app.MapEngenhariaModule();
app.MapFinanceiroModule();

// Background Consumers (EDA Monolito Modular)
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<EmissorDeFaturas>(
    nameof(EmissorDeFaturas),
    job => job.ExecutarAsync(),
    Cron.Minutely
);

recurringJobManager.AddOrUpdate<ReservadorDeConteiner>(
    nameof(ReservadorDeConteiner),
    job => job.ExecutarAsync(),
    Cron.Minutely
);

app.Run();