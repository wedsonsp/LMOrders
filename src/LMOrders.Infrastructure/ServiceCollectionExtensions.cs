using System.Threading.Channels;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Messaging;
using LMOrders.Infrastructure.Messaging.Kafka;
using LMOrders.Infrastructure.Mongo.Repositories;
using LMOrders.Infrastructure.Options;
using LMOrders.Infrastructure.Persistence.DbContexts;
using LMOrders.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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

        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "lmorders:";
            });
        }

        services.AddSingleton<IMongoClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MongoOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("A connection string do MongoDB não foi configurada.");
            }

            return new MongoClient(options.ConnectionString);
        });

        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IPedidoItemRepository, PedidoItemMongoRepository>();

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

        services.AddSingleton<IKafkaTopicResolver, KafkaTopicResolver>();
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
        services.AddHostedService<PedidoCriadoConsumerService>();

        return services;
    }
}

