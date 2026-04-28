using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Seguridad
{
    [Table("Usuarios")]
    public class UsuarioEntity : AuditoriaEntity
    {
        public DateTime? UltimoAcceso { get; set; }
        public int RolId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }
        public string? NombreCompleto { get; set; }
        public string? PasswordHash { get; set; }
        public bool EmailVerificado { get; set; }
        public bool Estado { get; set; }
        public string? Email { get; set; }
    }
}

