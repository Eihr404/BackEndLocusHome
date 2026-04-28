namespace Microservicio.Clientes.Business.DTOs.Clientes;

public class ActualizarClienteRequest
{
    public int ClienteId { get; set; }
    public string? Telefono { get; set; }
    public string? Domicilio { get; set; }
    public string? FotoUrl { get; set; }
}
