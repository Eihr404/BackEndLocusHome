using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    [Table("Clientes")]
    public class ClienteEntity : AuditoriaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClienteId { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [MaxLength(500)]
        public string? FotoUrl { get; set; }
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [MaxLength(300)]
        public string? Domicilio { get; set; }
        
        public int? CiudadId { get; set; }
        
        public decimal Calificacion { get; set; } = 5.0m;
        
        public int TotalReservas { get; set; } = 0;
        
        public int PuntosAcumulados { get; set; } = 0;
    }
}
