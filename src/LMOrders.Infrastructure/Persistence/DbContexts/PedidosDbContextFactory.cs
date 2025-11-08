using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LMOrders.Infrastructure.Persistence.DbContexts;

public class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // When running from Infrastructure project, we need to reach Api appsettings
        var apiBasePath = Path.Combine(basePath, "..", "LMOrders.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiBasePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<PedidosDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.MigrationsAssembly(typeof(PedidosDbContext).Assembly.FullName));

        return new PedidosDbContext(optionsBuilder.Options);
    }
}

