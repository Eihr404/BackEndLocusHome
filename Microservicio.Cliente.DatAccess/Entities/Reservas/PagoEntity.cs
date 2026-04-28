using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    public class PagoEntity : AuditoriaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PagoId { get; set; }
        public int? MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public int MonedaId { get; set; }
        public string? TipoPago { get; set; }
        public int ReservaId { get; set; }
        public string? ReferenciaPago { get; set; }
        public string? Estado { get; set; }
    }
}

