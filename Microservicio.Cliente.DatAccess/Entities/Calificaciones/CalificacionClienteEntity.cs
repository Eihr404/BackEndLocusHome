using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("CalificacionCliente")]
    public class CalificacionClienteEntity
    {
        public decimal Puntuacion { get; set; }
        public int ReservaId { get; set; }
        public int ClienteId { get; set; }
        public bool EsNoShow { get; set; }
        public string? Comentario { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CalificacionId { get; set; }
        public int ColaboradorId { get; set; }
        // Columnas parciales
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
        public string? IpOrigen { get; set; }
    }
}
