using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Seguridad
{
    /// <summary>
    /// Tabla intermedia N:N entre Usuario y Rol.
    /// Un usuario puede tener múltiples roles (Admin, Cliente, Colaborador).
    /// </summary>
    [Table("UsuarioRol")]
    public class UsuarioRolEntity : AuditoriaEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioRolId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int RolId { get; set; }

        // Navegación (EF Core resolverá el JOIN automáticamente)
        public UsuarioEntity? Usuario { get; set; }
        public RolEntity? Rol { get; set; }
    }
}
