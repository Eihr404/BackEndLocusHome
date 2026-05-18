using Microsoft.Extensions.DependencyInjection;
using Reservas.DataAccess.Repositories;
using Reservas.DataAccess.Repositories.Interfaces;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Services;
using Reservas.Business.Interfaces;
using Reservas.Business.Services;

namespace Reservas.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ── DataAccess ──────────────────────────────────
        services.AddScoped<IReservasRepository, ReservasRepository>();
        services.AddScoped<IDescuentosRepository, DescuentosRepository>();
        services.AddScoped<IReservaDetallesRepository, ReservaDetallesRepository>();

        // ── DataManagement ──────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReservasDataService, ReservasDataService>();
        services.AddScoped<IDescuentosDataService, DescuentosDataService>();

        // ── Business ────────────────────────────────────
        services.AddScoped<IReservasService, ReservasService>();

        return services;
    }
}
