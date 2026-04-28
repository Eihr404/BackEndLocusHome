namespace Microservicio.Clientes.Business.DTOs.Clientes;

/// <summary>Versión resumida del cliente para listados y búsquedas.</summary>
public class ClienteResumenResponse
{
    public int ClienteId { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public decimal Calificacion { get; set; }
    public int TotalReservas { get; set; }
}
