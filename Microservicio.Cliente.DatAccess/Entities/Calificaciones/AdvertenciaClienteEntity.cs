using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Calificaciones
{
    [Table("AdvertenciasCliente")]
    public class AdvertenciaClienteEntity
    {
        public string? Descripcion { get; set; }
        public int? ReservaId { get; set; }
        public int Severidad { get; set; }
        public string? Tipo { get; set; }
        public int ClienteId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdvertenciaId { get; set; }
        // Columnas parciales
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? UsuarioCreacion { get; set; }
        public bool EliminadoLogico { get; set; } = false;
        public string? IpOrigen { get; set; }
    }
}
