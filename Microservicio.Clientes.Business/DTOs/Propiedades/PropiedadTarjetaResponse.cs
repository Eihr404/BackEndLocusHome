namespace Microservicio.Clientes.Business.DTOs.Propiedades;

/// <summary>Versión tarjeta para el marketplace de búsqueda.</summary>
public class PropiedadTarjetaResponse
{
    public int PropiedadId { get; set; }
    public string? Nombre { get; set; }
    public string? Ciudad { get; set; }
    public int Estrellas { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public bool AdmiteMascotas { get; set; }
}
