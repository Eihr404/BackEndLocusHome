using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Core
{
    public class HabitacionEntity : AuditoriaEntity
    {
        public decimal? SuperficieM2 { get; set; }
        public int NumBanos { get; set; }
        public int CapacidadAdultos { get; set; }
        public string? Nombre { get; set; }
        public bool AdmiteMascotas { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HabitacionId { get; set; }
        public bool TieneCocina { get; set; }
        public int PropiedadId { get; set; }
        public int NumDormitorios { get; set; }
        public int CapacidadNinos { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
        public bool TieneAireAcondicionado { get; set; }
    }
}

