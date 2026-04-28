namespace Microservicio.Clientes.Business.DTOs.Clientes;

public class CrearClienteRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Domicilio { get; set; }
    public string? FotoUrl { get; set; }
}
