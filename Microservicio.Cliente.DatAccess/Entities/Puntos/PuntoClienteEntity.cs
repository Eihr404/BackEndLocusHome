using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Puntos
{
    [Table("PuntosCliente")]
    public class PuntoClienteEntity : AuditoriaEntity
    {
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }
        public int Cantidad { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PuntoId { get; set; }
        public int? ReservaId { get; set; }
        public int ClienteId { get; set; }
    }
}


