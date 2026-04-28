using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Seguridad
{
    [Table("Roles")]
    public class RolEntity : AuditoriaEntity
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RolId { get; set; }
        public bool Estado { get; set; }
    }
}

