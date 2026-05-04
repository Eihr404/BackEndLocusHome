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

            // Aplicar configuraciones de las entidades
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);

            // ── SQL Server: Declarar triggers existentes ─────────────────────
            // EF Core 7+ requiere declarar explícitamente los triggers para que
            // use la estrategia "sequence" en vez de OUTPUT INSERTED (incompatible con triggers)
            modelBuilder.Entity<UsuarioEntity>()
                .ToTable(tb => tb.HasTrigger("trg_Audit_Usuarios"));

            modelBuilder.Entity<ClienteEntity>()
                .ToTable(tb => tb.HasTrigger("trg_Audit_Clientes"));

            modelBuilder.Entity<PropiedadEntity>()
                .ToTable(tb => tb.HasTrigger("trg_Audit_Propiedades"));

            modelBuilder.Entity<ReservaEntity>()
                .ToTable(tb => tb.HasTrigger("trg_Audit_Reservas"));

            modelBuilder.Entity<EncuestaExperienciaEntity>()
                .ToTable(tb => tb.HasTrigger("trg_Puntos_Encuesta"));

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
        }
    }
}
