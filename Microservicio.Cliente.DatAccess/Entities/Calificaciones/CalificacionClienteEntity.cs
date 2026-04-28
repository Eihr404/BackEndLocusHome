using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("CalificacionCliente")]
    public class CalificacionClienteEntity : AuditoriaEntity
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
    }
}


