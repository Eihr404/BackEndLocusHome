using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class PropiedadInstalacionEntity : AuditoriaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropiedadInstalacionId { get; set; }
        public int PropiedadId { get; set; }
        public int InstalacionId { get; set; }
    }
}

