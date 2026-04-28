using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class TarifaEntity : AuditoriaEntity
    {
        public DateTime FechaFin { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TarifaId { get; set; }
        public int HabitacionId { get; set; }
        public int MonedaId { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public DateTime FechaInicio { get; set; }
    }
}

