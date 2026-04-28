namespace Microservicio.Clientes.Business.DTOs.Colaboradores;

public class CrearColaboradorRequest
{
    public int UsuarioId { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? Telefono { get; set; }
    public string? CuentaBancaria { get; set; }
    public string? FotoUrl { get; set; }
}
