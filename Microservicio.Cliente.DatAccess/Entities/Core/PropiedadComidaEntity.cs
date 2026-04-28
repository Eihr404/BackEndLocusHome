using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class PropiedadComidaEntity : AuditoriaEntity
    {
        public decimal PrecioAdicional { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropiedadComidaId { get; set; }
        public int PropiedadId { get; set; }
        public int ComidaId { get; set; }
    }
}

