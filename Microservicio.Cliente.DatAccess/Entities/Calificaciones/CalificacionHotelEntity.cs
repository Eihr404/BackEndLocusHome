using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("CalificacionHotel")]
    public class CalificacionHotelEntity : AuditoriaEntity
    {
        public string? Comentario { get; set; }
        public int ReservaId { get; set; }
        public decimal Puntuacion { get; set; }
        public int PropiedadId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CalificacionId { get; set; }
        public int ClienteId { get; set; }
    }
}


