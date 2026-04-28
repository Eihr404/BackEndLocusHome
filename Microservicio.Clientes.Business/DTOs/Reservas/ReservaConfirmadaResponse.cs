namespace Microservicio.Clientes.Business.DTOs.Reservas;

/// <summary>Confirmación al crear una reserva — lo que ve el usuario inmediatamente.</summary>
public class ReservaConfirmadaResponse
{
    public string? CodigoReserva { get; set; }
    public string Mensaje { get; set; } = "Reserva creada exitosamente.";
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public decimal Total { get; set; }
}
