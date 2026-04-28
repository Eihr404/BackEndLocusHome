using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Puntos
{
    [Table("ComisionPlataforma")]
    public class ComisionPlataformaEntity : AuditoriaEntity
    {
        public string? Descripcion { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComisionId { get; set; }
        public DateTime FechaInicio { get; set; }
    }
}


