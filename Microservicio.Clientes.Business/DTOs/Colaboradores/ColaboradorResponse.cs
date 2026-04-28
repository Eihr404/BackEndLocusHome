namespace Microservicio.Clientes.Business.DTOs.Colaboradores;

public class ColaboradorResponse
{
    public int ColaboradorId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreCompleto { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
    public bool Verificado { get; set; }
    public int PuntosAcumulados { get; set; }
    public int TotalPropiedades { get; set; }
}
