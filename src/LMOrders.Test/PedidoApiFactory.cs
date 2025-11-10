using System.Linq;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Persistence.DbContexts;
using LMOrders.Test.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace LMOrders.Test;

public class PedidoApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"PedidosTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PedidosDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<PedidosDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            var itensRepositoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IPedidoItemRepository));

            if (itensRepositoryDescriptor is not null)
            {
                services.Remove(itensRepositoryDescriptor);
            }

            var kafkaProducerDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IKafkaProducerService));
            if (kafkaProducerDescriptor is not null)
            {
                services.Remove(kafkaProducerDescriptor);
            }

            var kafkaTopicResolverDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IKafkaTopicResolver));
            if (kafkaTopicResolverDescriptor is not null)
            {
                services.Remove(kafkaTopicResolverDescriptor);
            }

            var distributedCacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDistributedCache));
            if (distributedCacheDescriptor is not null)
            {
                services.Remove(distributedCacheDescriptor);
            }

            services.AddDistributedMemoryCache();

            services.AddSingleton<InMemoryPedidoItemRepository>();
            services.AddSingleton<IPedidoItemRepository>(provider => provider.GetRequiredService<InMemoryPedidoItemRepository>());

            services.AddSingleton<FakeKafkaProducerService>();
            services.AddSingleton<IKafkaProducerService>(provider => provider.GetRequiredService<FakeKafkaProducerService>());
            services.AddSingleton<IKafkaTopicResolver, FakeKafkaTopicResolver>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

