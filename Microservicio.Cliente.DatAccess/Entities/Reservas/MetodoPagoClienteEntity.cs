using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    [Table("MetodosPagoCliente")]
    public class MetodoPagoClienteEntity : AuditoriaEntity
    {
        public int ClienteId { get; set; }
        public bool EsPrincipal { get; set; }
        public string? NumeroTarjeta { get; set; }
        public string? Tipo { get; set; }
        public string? NombreTitular { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MetodoPagoId { get; set; }
        public bool Estado { get; set; }
        public string? FechaExpiracion { get; set; }
    }
}


