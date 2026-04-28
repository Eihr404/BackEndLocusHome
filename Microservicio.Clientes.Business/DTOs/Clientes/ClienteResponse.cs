namespace Microservicio.Clientes.Business.DTOs.Clientes;

public class ClienteResponse
{
    public int ClienteId { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
    public string? Domicilio { get; set; }
    public decimal Calificacion { get; set; }
    public int TotalReservas { get; set; }
    public int PuntosAcumulados { get; set; }
    public DateTime FechaRegistro { get; set; }
}
