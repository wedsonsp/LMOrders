using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Persistence.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

            services.AddSingleton<InMemoryPedidoItemRepository>();
            services.AddSingleton<IPedidoItemRepository>(provider => provider.GetRequiredService<InMemoryPedidoItemRepository>());

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

