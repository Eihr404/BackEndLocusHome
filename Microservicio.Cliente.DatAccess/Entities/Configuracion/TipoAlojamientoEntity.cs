using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    public class TipoAlojamientoEntity : AuditoriaEntity
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoAlojamientoId { get; set; }
        public bool Estado { get; set; }
    }
}

