using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    [Table("MetodosPagoCliente")]
    public class MetodoPagoClienteEntity
    {
        public int ClienteId { get; set; }
        public string? Tipo { get; set; }
        public string? NumeroTarjeta { get; set; }
        public string? NombreTitular { get; set; }
        public string? FechaExpiracion { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MetodoPagoId { get; set; }
        // Columnas parciales de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
        public string? IpOrigen { get; set; }
    }
}
