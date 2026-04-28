using Microsoft.EntityFrameworkCore;
using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Calificaciones;
using Microservicio.Cliente.DatAccess.Entities.Configuracion;
using Microservicio.Cliente.DatAccess.Entities.Puntos;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;

namespace Microservicio.Cliente.DatAccess.Contexts
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        // ── Módulo Geográfico y Monedas ────────────────────────────────────
        public DbSet<PaisEntity> Paises { get; set; }
        public DbSet<CiudadEntity> Ciudades { get; set; }
        public DbSet<MonedaEntity> Monedas { get; set; }
        public DbSet<TasaCambioEntity> TasasCambio { get; set; }

        // ── Módulo Seguridad y Usuarios ────────────────────────────────────
        public DbSet<RolEntity> Roles { get; set; }
        public DbSet<UsuarioEntity> Usuarios { get; set; }
        public DbSet<ClienteEntity> Clientes { get; set; }
        public DbSet<ColaboradorEntity> Colaboradores { get; set; }

        // ── Módulo Propiedades/Alojamientos ────────────────────────────────
        public DbSet<TipoAlojamientoEntity> TiposAlojamiento { get; set; }
        public DbSet<PropiedadEntity> Propiedades { get; set; }
        public DbSet<PropiedadFotoEntity> PropiedadFotos { get; set; }
        public DbSet<CatalogoInstalacionEntity> CatalogoInstalaciones { get; set; }
        public DbSet<PropiedadInstalacionEntity> PropiedadInstalaciones { get; set; }
        public DbSet<CatalogoComidaEntity> CatalogoComidas { get; set; }
        public DbSet<PropiedadComidaEntity> PropiedadComidas { get; set; }
        public DbSet<HabitacionEntity> Habitaciones { get; set; }
        public DbSet<HabitacionFotoEntity> HabitacionFotos { get; set; }
        public DbSet<TarifaEntity> Tarifas { get; set; }
        public DbSet<DisponibilidadEntity> Disponibilidad { get; set; }

        // ── Módulo Reservas ────────────────────────────────────────────────
        public DbSet<MetodoPagoClienteEntity> MetodosPagoCliente { get; set; }
        public DbSet<ReservaEntity> Reservas { get; set; }
        public DbSet<ReservaDetalleHabitacionEntity> ReservaDetalleHabitacion { get; set; }
        public DbSet<PagoEntity> Pagos { get; set; }
        public DbSet<CorreoVerificacionEntity> CorreosVerificacion { get; set; }

        // ── Módulo Calificaciones y Advertencias ───────────────────────────
        public DbSet<CalificacionHotelEntity> CalificacionHotel { get; set; }
        public DbSet<CalificacionClienteEntity> CalificacionCliente { get; set; }
        public DbSet<AdvertenciaClienteEntity> AdvertenciasCliente { get; set; }
        public DbSet<EncuestaExperienciaEntity> EncuestaExperiencia { get; set; }

        // ── Módulo Puntos, Promociones y Descuentos ────────────────────────
        public DbSet<PuntoClienteEntity> PuntosCliente { get; set; }
        public DbSet<PuntoColaboradorEntity> PuntosColaborador { get; set; }
        public DbSet<ComisionPlataformaEntity> ComisionPlataforma { get; set; }
        public DbSet<PromocionEntity> Promociones { get; set; }
        public DbSet<DescuentoEntity> Descuentos { get; set; }

        // ── Auditoría ──────────────────────────────────────────────────────
        public DbSet<AuditoriaGeneralEntity> AuditoriaGeneral { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Carga automática de todas las clases IEntityTypeConfiguration<T>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);

            // Forzar nombres de tablas y columnas en minúsculas para PostgreSQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()!.ToLower());

                foreach (var property in entity.GetProperties())
                {
                    // Forzar nombre de columna a minúsculas
                    property.SetColumnName(property.Name.ToLower());
                }
            }

            // ── Usuarios ───────────────────────────────────────────────────
            // Email único
            modelBuilder.Entity<UsuarioEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relación Usuario → Rol (FK directa en la tabla Usuarios)
            modelBuilder.Entity<UsuarioEntity>()
                .HasOne<RolEntity>()
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostgreSQL maneja RETURNING automáticamente — no requiere HasTrigger
        }
    }
}
