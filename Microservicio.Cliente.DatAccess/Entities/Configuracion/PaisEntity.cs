using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    public class PaisEntity : AuditoriaEntity
    {
        public bool Estado { get; set; }
        public string? Nombre { get; set; }
        public string? CodigoISO { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaisId { get; set; }
    }
}

