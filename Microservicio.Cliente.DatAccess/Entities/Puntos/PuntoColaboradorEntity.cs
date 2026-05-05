using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Puntos
{
    [Table("PuntosColaborador")]
    public class PuntoColaboradorEntity
    {
        public string? Descripcion { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PuntoId { get; set; }
        public int Cantidad { get; set; }
        public string? Tipo { get; set; }
        public int ColaboradorId { get; set; }
        // Solo FechaCreacion existe en la tabla
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
