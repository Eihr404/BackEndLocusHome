namespace Microservicio.Clientes.DataManagement.Models;

/// <summary>
/// Modelo de Reserva para la Capa 2.
/// </summary>
public class ReservaDataModel
{
    public int ReservaId { get; set; }
    public string? CodigoReserva { get; set; }
    public int ClienteId { get; set; }
    public string? NombreCliente { get; set; }
    public int PropiedadId { get; set; }
    public string? NombrePropiedad { get; set; }
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
    public bool LlevaMascotas { get; set; }
    public decimal Total { get; set; }
    public string? Estado { get; set; }
    public string? Moneda { get; set; }
}

/// <summary>
/// Modelo para crear una nueva reserva desde la API.
/// </summary>
public class CrearReservaDataModel
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
