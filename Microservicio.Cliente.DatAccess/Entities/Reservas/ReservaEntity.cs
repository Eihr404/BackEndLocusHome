using Microservicio.Cliente.DatAccess.Entities.Auditoria;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    public class ReservaEntity : AuditoriaEntity
    {
        public int ClienteId { get; set; }
        public decimal Descuento { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public int NumAdultos { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public bool LlevaMascotas { get; set; }
        public string? CodigoReserva { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservaId { get; set; }
        public int NumHabitaciones { get; set; }
        public int PropiedadId { get; set; }
        public int NumNinos { get; set; }
        public int MonedaId { get; set; }
        public decimal SubTotal { get; set; }
        public string? Estado { get; set; }
        public decimal Total { get; set; }
    }
}

