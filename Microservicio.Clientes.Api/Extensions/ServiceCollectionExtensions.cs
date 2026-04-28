using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Services;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.Api.Extensions;

/// <summary>
/// Registra todos los servicios de las capas 2 y 3 en el contenedor DI.
/// Centraliza la inyección de dependencias, manteniendo Program.cs limpio.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookingServices(this IServiceCollection services, IConfiguration config)
    {
        // ── Capa 1: DbContext ─────────────────────────────────────────
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("BookingDB")));

        // ── Capa 2: DataManagement ────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClienteDataService, ClienteDataService>();
        services.AddScoped<IPropiedadDataService, PropiedadDataService>();
        services.AddScoped<IReservaDataService, ReservaDataService>();

        // ── Capa 3: Business ──────────────────────────────────────────
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IPropiedadService, PropiedadService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IAuthService, AuthService>();
        
        // Fase 1
        services.AddScoped<IMaestrosService, MaestrosService>();
        services.AddScoped<IColaboradoresService, ColaboradoresService>();
        services.AddScoped<IHabitacionesService, HabitacionesService>();

        // Fase 2
        services.AddScoped<IPagosService, PagosService>();
        services.AddScoped<ICalificacionesService, CalificacionesService>();

        return services;
    }
}
