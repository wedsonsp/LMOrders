using AutoMapper;
using LMOrders.Application.Interfaces.Services;
using LMOrders.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LMOrders.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<IPedidoApplicationService, PedidoApplicationService>();

        return services;
    }
}

