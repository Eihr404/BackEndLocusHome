namespace Microservicio.Clientes.Business.DTOs.Reservas;

/// <summary>Resumen de reserva para historial del cliente.</summary>
public class ReservaResumenResponse
{
    public int ReservaId { get; set; }
    public string? CodigoReserva { get; set; }
    public string? NombrePropiedad { get; set; }
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public decimal Total { get; set; }
    public string? Estado { get; set; }
}
