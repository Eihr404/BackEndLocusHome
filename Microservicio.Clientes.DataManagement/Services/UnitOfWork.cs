using Microsoft.EntityFrameworkCore;
using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Calificaciones;
using Microservicio.Cliente.DatAccess.Entities.Configuracion;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Contracts;
using Microservicio.Cliente.DatAccess.Repositories.Implementations;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.DataManagement.Services;

/// <summary>
/// Implementación concreta de Unit of Work.
/// Todos los repositorios comparten el mismo DbContext → misma transacción.
/// RolId está directamente en la tabla Usuarios — no hay tabla UsuarioRoles.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BookingDbContext _context;

    public IClienteRepository Clientes { get; }
    public IUsuarioRepository Usuarios { get; }
    public IRepository<RolEntity> Roles { get; }
    public IRepository<PropiedadEntity> Propiedades { get; }
    public IRepository<ReservaEntity> Reservas { get; }
    
    // Fase 1: Entidades
    public IRepository<CiudadEntity> Ciudades { get; }
    public IRepository<MonedaEntity> Monedas { get; }
    public IRepository<TipoAlojamientoEntity> TiposAlojamiento { get; }
    public IRepository<CatalogoInstalacionEntity> Instalaciones { get; }
    public IRepository<ColaboradorEntity> Colaboradores { get; }
    public IRepository<HabitacionEntity> Habitaciones { get; }
    public IRepository<DisponibilidadEntity> Disponibilidades { get; }

    // Fase 2: Entidades
    public IRepository<PagoEntity> Pagos { get; }
    public IRepository<CalificacionHotelEntity> CalificacionesHotel { get; }
    public IRepository<ReservaDetalleHabitacionEntity> ReservaDetalles { get; }
    public IRepository<TarifaEntity> Tarifas { get; }

    public UnitOfWork(BookingDbContext context)
    {
        _context    = context;
        Clientes    = new ClienteRepository(context);
        Usuarios    = new UsuarioRepository(context);
        Roles       = new GenericRepository<RolEntity>(context);
        Propiedades = new GenericRepository<PropiedadEntity>(context);
        Reservas    = new GenericRepository<ReservaEntity>(context);
        
        // Fase 1
        Ciudades         = new GenericRepository<CiudadEntity>(context);
        Monedas          = new GenericRepository<MonedaEntity>(context);
        TiposAlojamiento = new GenericRepository<TipoAlojamientoEntity>(context);
        Instalaciones    = new GenericRepository<CatalogoInstalacionEntity>(context);
        Colaboradores    = new GenericRepository<ColaboradorEntity>(context);
        Habitaciones     = new GenericRepository<HabitacionEntity>(context);
        Disponibilidades = new GenericRepository<DisponibilidadEntity>(context);

        // Fase 2
        Pagos               = new GenericRepository<PagoEntity>(context);
        CalificacionesHotel = new GenericRepository<CalificacionHotelEntity>(context);
        ReservaDetalles     = new GenericRepository<ReservaDetalleHabitacionEntity>(context);
        Tarifas             = new GenericRepository<TarifaEntity>(context);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        => await _context.Database.ExecuteSqlRawAsync(sql, parameters);

    public void Dispose()
        => _context.Dispose();
}
