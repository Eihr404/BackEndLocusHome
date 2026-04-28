namespace Microservicio.Clientes.Business.DTOs.Habitaciones;

public class HabitacionResponse
{
    public int HabitacionId { get; set; }
    public int PropiedadId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public int NumDormitorios { get; set; }
    public int NumBanos { get; set; }
    public decimal? SuperficieM2 { get; set; }
    public bool AdmiteMascotas { get; set; }
    public bool TieneCocina { get; set; }
    public bool TieneAireAcondicionado { get; set; }
    public bool Disponible { get; set; }
}
