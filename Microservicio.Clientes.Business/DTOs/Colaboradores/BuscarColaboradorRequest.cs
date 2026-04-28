namespace Microservicio.Clientes.Business.DTOs.Colaboradores;

public class BuscarColaboradorRequest
{
    public string? NombreEmpresa { get; set; }
    public bool? SoloVerificados { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
