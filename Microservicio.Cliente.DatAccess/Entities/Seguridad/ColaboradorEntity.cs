using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Seguridad
{
    public class ColaboradorEntity : AuditoriaEntity
    {
        public bool Verificado { get; set; }
        public int UsuarioId { get; set; }
        public string? Telefono { get; set; }
        public int PuntosAcumulados { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ColaboradorId { get; set; }
        public string? CuentaBancaria { get; set; }
        public string? FotoUrl { get; set; }
        public string? NombreEmpresa { get; set; }
    }
}

