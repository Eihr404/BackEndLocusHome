namespace Microservicio.Clientes.Business.DTOs.Clientes;

public class BuscarClienteRequest
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public decimal? CalificacionMinima { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
