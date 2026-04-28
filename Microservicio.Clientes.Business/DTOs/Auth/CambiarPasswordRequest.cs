namespace Microservicio.Clientes.Business.DTOs.Auth;

public class CambiarPasswordRequest
{
    public int UsuarioId { get; set; }
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
    public string ConfirmacionPassword { get; set; } = string.Empty;
}
