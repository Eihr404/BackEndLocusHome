using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservas.API.Integrations;
using Reservas.Business.Interfaces;
using Reservas.Business.Services;
using Reservas.DataAccess.Repositories;
using Reservas.DataAccess.Repositories.Interfaces;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Services;

namespace Reservas.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IReservasRepository, ReservasRepository>();
        services.AddScoped<IDescuentosRepository, DescuentosRepository>();
        services.AddScoped<IReservaDetallesRepository, ReservaDetallesRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReservasDataService, ReservasDataService>();
        services.AddScoped<IDescuentosDataService, DescuentosDataService>();

        services.AddScoped<IReservasService, ReservasService>();

        var grpcUrl = configuration["GrpcUrls:Alojamientos"] ?? "http://localhost:5002";
        var alojamientosApiUrl = configuration["ServiceUrls:AlojamientosApi"] ?? $"{grpcUrl.TrimEnd('/')}/api/v1/";

        services.AddGrpcClient<Shared.Protos.CalendarioGrpc.CalendarioGrpcClient>(options =>
        {
            options.Address = new Uri(grpcUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
            new Grpc.Net.Client.Web.GrpcWebHandler(
                Grpc.Net.Client.Web.GrpcWebMode.GrpcWeb,
                new HttpClientHandler()
            ));

        services.AddHttpClient<CalendarioGateway>(client =>
        {
            client.BaseAddress = new Uri(alojamientosApiUrl);
        });

        services.AddScoped<ICalendarioGateway>(serviceProvider =>
            serviceProvider.GetRequiredService<CalendarioGateway>());

        return services;
    }
}
