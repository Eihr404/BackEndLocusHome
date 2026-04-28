using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    [Table("CorreosVerificacion")]
    public class CorreoVerificacionEntity : AuditoriaEntity
    {
        public string? Tipo { get; set; }
        public int UsuarioId { get; set; }
        public string? Token { get; set; }
        public int? ReservaId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VerificacionId { get; set; }
        public bool Verificado { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
    }
}


