namespace Microservicio.Clientes.Business.DTOs.Propiedades;

public class ActualizarPropiedadRequest
{
    public int PropiedadId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool AdmiteMascotas { get; set; }
    public int Estrellas { get; set; }
    public string? Direccion { get; set; }
    public string? Estado { get; set; }
}
