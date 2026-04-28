namespace Microservicio.Clientes.DataManagement.Models;

/// <summary>
/// Modelo de datos de Propiedad/Alojamiento para la Capa 2.
/// </summary>
public class PropiedadDataModel
{
    public int PropiedadId { get; set; }
    public int ColaboradorId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public int Estrellas { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalResenas { get; set; }
    public bool AdmiteMascotas { get; set; }
    public string? Estado { get; set; }
    public string? TipoAlojamiento { get; set; }
}

/// <summary>
/// Filtros para búsqueda avanzada de propiedades/alojamientos.
/// </summary>
public class PropiedadFiltroDataModel
{
    public int? CiudadId { get; set; }
    public bool? AdmiteMascotas { get; set; }
    public int? EstrellasMinimas { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public int NumAdultos { get; set; } = 1;
    public int NumNinos { get; set; } = 0;
    public DateTime? FechaCheckIn { get; set; }
    public DateTime? FechaCheckOut { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
