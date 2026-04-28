using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class PropiedadEntity : AuditoriaEntity
    {
        public int TotalResenas { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public bool Verificada { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Latitud { get; set; }
        public string? Nombre { get; set; }
        public bool AdmiteMascotas { get; set; }
        public int? Estrellas { get; set; }
        public int ColaboradorId { get; set; }
        public int ClicksAnuncio { get; set; }
        public int CiudadId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropiedadId { get; set; }
        public decimal? Longitud { get; set; }
        public int TipoAlojamientoId { get; set; }
        public string? Estado { get; set; }
        public string? Direccion { get; set; }
    }
}

