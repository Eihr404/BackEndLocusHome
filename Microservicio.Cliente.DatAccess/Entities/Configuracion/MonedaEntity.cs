using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    public class MonedaEntity : AuditoriaEntity
    {
        public string? Nombre { get; set; }
        public bool Estado { get; set; }
        public int PaisId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MonedaId { get; set; }
        public string? Simbolo { get; set; }
        public string? Codigo { get; set; }
    }
}

