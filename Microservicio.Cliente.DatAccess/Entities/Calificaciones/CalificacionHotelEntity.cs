using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("CalificacionHotel")]
    public class CalificacionHotelEntity
    {
        public string? Comentario { get; set; }
        public int ReservaId { get; set; }
        public decimal Puntuacion { get; set; }
        public int PropiedadId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CalificacionId { get; set; }
        public int ClienteId { get; set; }
        // Columnas parciales
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
        public string? IpOrigen { get; set; }
    }
}
