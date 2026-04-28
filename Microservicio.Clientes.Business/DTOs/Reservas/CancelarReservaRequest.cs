namespace Microservicio.Clientes.Business.DTOs.Reservas;

public class CancelarReservaRequest
{
    public int ReservaId { get; set; }
    public int ClienteId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
