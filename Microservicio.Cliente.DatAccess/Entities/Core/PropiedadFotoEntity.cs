using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    [Table("PropiedadFotos")]
    public class PropiedadFotoEntity
    {
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FotoId { get; set; }
        public int PropiedadId { get; set; }
        public string? Url { get; set; }
        public bool EsPrincipal { get; set; }
        // Columnas parciales de auditoría (solo estas existen en la tabla)
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
    }
}
