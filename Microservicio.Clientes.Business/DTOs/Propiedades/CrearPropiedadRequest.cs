namespace Microservicio.Clientes.Business.DTOs.Propiedades;

public class CrearPropiedadRequest
{
    public int ColaboradorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CiudadId { get; set; }
    public int TipoAlojamientoId { get; set; }
    public int Estrellas { get; set; }
    public bool AdmiteMascotas { get; set; }
    public string? Direccion { get; set; }
}
