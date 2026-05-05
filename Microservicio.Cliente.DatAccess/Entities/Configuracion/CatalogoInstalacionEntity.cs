using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    [Table("CatalogoInstalaciones")]
    public class CatalogoInstalacionEntity
    {
        public string? Icono { get; set; }
        public string? Nombre { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InstalacionId { get; set; }
    }
}
