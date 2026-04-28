namespace Microservicio.Clientes.Business.Events;

public class ReservaConfirmadaIntegrationEvent
{
    public string CodigoReserva { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int PropiedadId { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaConfirmacion { get; set; } = DateTime.UtcNow;
}
