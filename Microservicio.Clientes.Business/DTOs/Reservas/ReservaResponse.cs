namespace Microservicio.Clientes.Business.DTOs.Reservas;

public class ReservaResponse
{
    public int ReservaId { get; set; }
    public string? CodigoReserva { get; set; }
    public int ClienteId { get; set; }
    public string? NombreCliente { get; set; }
    public int PropiedadId { get; set; }
    public string? NombrePropiedad { get; set; }
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NochesTotal => (FechaCheckOut - FechaCheckIn).Days;
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
    public bool LlevaMascotas { get; set; }
    public decimal Total { get; set; }
    public string? Estado { get; set; }
}
