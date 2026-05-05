using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas
{
    [Table("ReservaDetalleHabitacion")]
    public class ReservaDetalleHabitacionEntity
    {
        public decimal SubTotalHabitacion { get; set; }
        public int ReservaId { get; set; }
        public int HabitacionId { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public int NumNoches { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DetalleId { get; set; }
    }
}
