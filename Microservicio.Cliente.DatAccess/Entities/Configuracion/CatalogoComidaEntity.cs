using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Configuracion
{
    [Table("CatalogoComidas")]
    public class CatalogoComidaEntity
    {
        public string? Descripcion { get; set; }
        public string? Nombre { get; set; }
        public bool Estado { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComidaId { get; set; }
    }
}
