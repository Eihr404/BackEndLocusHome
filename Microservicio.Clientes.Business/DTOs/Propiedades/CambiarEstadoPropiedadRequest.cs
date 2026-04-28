namespace Microservicio.Clientes.Business.DTOs.Propiedades;

public class CambiarEstadoPropiedadRequest
{
    public int PropiedadId { get; set; }
    public string NuevoEstado { get; set; } = string.Empty; // Activa | Inactiva | EnMantenimiento
}
