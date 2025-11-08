using System.Threading.Channels;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Messaging;
using LMOrders.Infrastructure.Persistence.DbContexts;
using LMOrders.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMOrders.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PedidosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(PedidosDbContext).Assembly.FullName)));

        services.AddScoped<IPedidoRepository, PedidoRepository>();

        services.AddSingleton(provider =>
        {
            var channel = Channel.CreateUnbounded<BillingMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            return channel;
        });

        services.AddSingleton(provider => provider.GetRequiredService<Channel<BillingMessage>>().Writer);
        services.AddSingleton(provider => provider.GetRequiredService<Channel<BillingMessage>>().Reader);

        services.AddScoped<IBillingIntegrationService, BillingIntegrationService>();
        services.AddHostedService<BillingMessageDispatcher>();

        return services;
    }
}

