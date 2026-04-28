using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    [Table("CatalogoInstalaciones")]
    public class CatalogoInstalacionEntity : AuditoriaEntity
    {
        public string? Icono { get; set; }
        public string? Nombre { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InstalacionId { get; set; }
    }
}


