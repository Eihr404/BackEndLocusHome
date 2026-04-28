namespace Microservicio.Clientes.Business.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public int? ClienteId { get; set; }
    public int? ColaboradorId { get; set; }
    public List<string> Roles { get; set; } = new();
}
