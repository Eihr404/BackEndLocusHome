namespace Microservicio.Clientes.Business.DTOs.Propiedades;

public class BuscarPropiedadRequest
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
