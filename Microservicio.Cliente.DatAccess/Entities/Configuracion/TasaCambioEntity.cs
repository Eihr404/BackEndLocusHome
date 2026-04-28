using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    public class TasaCambioEntity : AuditoriaEntity
    {
        public int MonedaDestinoId { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TasaCambioId { get; set; }
        public int MonedaOrigenId { get; set; }
        public decimal Tasa { get; set; }
        public DateTime FechaVigencia { get; set; }
    }
}

