namespace Microservicio.Clientes.Business.DTOs.Reservas;

public class BuscarReservaRequest
{
    public int? ClienteId { get; set; }
    public int? PropiedadId { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
