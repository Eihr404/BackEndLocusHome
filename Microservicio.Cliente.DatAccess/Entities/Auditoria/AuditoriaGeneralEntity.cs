using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Auditoria
{
    [Table("AuditoriaGeneral")]
    public class AuditoriaGeneralEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditoriaId { get; set; }

        public string? NombreTabla { get; set; }
        public string? RegistroId { get; set; }
        public string? Operacion { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
        public string? UsuarioAccion { get; set; }
        public string? IpOrigen { get; set; }
        public DateTime FechaAccion { get; set; }
    }
}
