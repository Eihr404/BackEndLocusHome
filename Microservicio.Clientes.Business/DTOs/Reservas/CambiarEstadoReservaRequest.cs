namespace Microservicio.Clientes.Business.DTOs.Reservas;

public class CambiarEstadoReservaRequest
{
    public int ReservaId { get; set; }
    public string NuevoEstado { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}
