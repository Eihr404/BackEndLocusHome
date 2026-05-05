using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class DisponibilidadEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DisponibilidadId { get; set; }
        public DateTime Fecha { get; set; }
        public bool Disponible { get; set; }
        public int HabitacionId { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}

