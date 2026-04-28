using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Puntos
{
    public class DescuentoEntity : AuditoriaEntity
    {
        public int? ClienteId { get; set; }
        public int? PromocionId { get; set; }
        public string? Origen { get; set; }
        public decimal Porcentaje { get; set; }
        public bool AprobadoPorDueno { get; set; }
        public int? ReservaId { get; set; }
        public decimal MontoDescuento { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DescuentoId { get; set; }
        public bool Estado { get; set; }
    }
}

