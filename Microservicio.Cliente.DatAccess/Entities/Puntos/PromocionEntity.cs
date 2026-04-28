using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Puntos
{
    public class PromocionEntity : AuditoriaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromocionId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Nombre { get; set; }
        public int PropiedadId { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}

