using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("EncuestaExperiencia")]
    public class EncuestaExperienciaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EncuestaId { get; set; }
        public int PuntosOtorgados { get; set; }
        public int ClienteId { get; set; }
        public decimal CalificacionGeneral { get; set; }
        public int PropiedadId { get; set; }
        public decimal? Limpieza { get; set; }
        public int ReservaId { get; set; }
        public decimal? RelacionCalidadPrecio { get; set; }
        public decimal? Servicio { get; set; }
        public decimal? Ubicacion { get; set; }
        public string? Comentarios { get; set; }
        // Columnas parciales
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
        public string? IpOrigen { get; set; }
    }
}
