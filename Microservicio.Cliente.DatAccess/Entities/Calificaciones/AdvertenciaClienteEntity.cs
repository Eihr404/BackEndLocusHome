using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("AdvertenciasCliente")]
    public class AdvertenciaClienteEntity : AuditoriaEntity
    {
        public string? Descripcion { get; set; }
        public int? ReservaId { get; set; }
        public int Severidad { get; set; }
        public string? Tipo { get; set; }
        public int ClienteId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdvertenciaId { get; set; }
    }
}


