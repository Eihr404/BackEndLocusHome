namespace Microservicio.Clientes.Business.DTOs.Propiedades;

public class PropiedadResponse
{
    public int PropiedadId { get; set; }
    public int ColaboradorId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public int Estrellas { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalResenas { get; set; }
    public bool AdmiteMascotas { get; set; }
    public string? Estado { get; set; }
}
