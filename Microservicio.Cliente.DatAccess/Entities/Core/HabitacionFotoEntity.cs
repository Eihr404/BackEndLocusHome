using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    [Table("HabitacionFotos")]
    public class HabitacionFotoEntity
    {
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FotoId { get; set; }
        public int HabitacionId { get; set; }
        public string? Url { get; set; }
        public bool EsPrincipal { get; set; }
        // Columnas parciales de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool EliminadoLogico { get; set; } = false;
    }
}
