namespace Microservicio.Clientes.Business.DTOs.Reservas;

public class CrearReservaRequest
{
    public int ClienteId { get; set; }
    public int PropiedadId { get; set; }
    public List<int> HabitacionIds { get; set; } = new();
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
    public bool LlevaMascotas { get; set; }
    public int MonedaId { get; set; }
    public int MetodoPagoId { get; set; }
}
