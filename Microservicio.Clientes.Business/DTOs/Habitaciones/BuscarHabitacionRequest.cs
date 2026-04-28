namespace Microservicio.Clientes.Business.DTOs.Habitaciones;

public class BuscarHabitacionRequest
{
    public int PropiedadId { get; set; }
    public int NumAdultos { get; set; } = 1;
    public int NumNinos { get; set; } = 0;
    public bool? AdmiteMascotas { get; set; }
    public bool? TieneCocina { get; set; }
    public DateTime? FechaCheckIn { get; set; }
    public DateTime? FechaCheckOut { get; set; }
}
