using Microservicio.Cliente.DatAccess.Entities.Calificaciones;
using Microservicio.Cliente.DatAccess.Entities.Configuracion;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Contracts;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;

namespace Microservicio.Clientes.DataManagement.Interfaces;

/// <summary>
/// Unidad de Trabajo: agrupa todos los repositorios bajo una transacción compartida.
/// Usuarios.RolId es FK directa — no hay tabla UsuarioRoles en la BD.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IUsuarioRepository Usuarios { get; }
    IRepository<RolEntity> Roles { get; }
    IRepository<PropiedadEntity> Propiedades { get; }
    IRepository<ReservaEntity> Reservas { get; }
    
    // Fase 1: Entidades
    IRepository<CiudadEntity> Ciudades { get; }
    IRepository<PaisEntity> Paises { get; }
    IRepository<MonedaEntity> Monedas { get; }
    IRepository<TipoAlojamientoEntity> TiposAlojamiento { get; }
    IRepository<CatalogoInstalacionEntity> Instalaciones { get; }
    IRepository<ColaboradorEntity> Colaboradores { get; }
    IRepository<HabitacionEntity> Habitaciones { get; }
    IRepository<DisponibilidadEntity> Disponibilidades { get; }

    // Fase 2: Entidades
    IRepository<PagoEntity> Pagos { get; }
    IRepository<CalificacionHotelEntity> CalificacionesHotel { get; }
    IRepository<ReservaDetalleHabitacionEntity> ReservaDetalles { get; }
    IRepository<TarifaEntity> Tarifas { get; }

    Task<int> SaveChangesAsync();
    Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
}
