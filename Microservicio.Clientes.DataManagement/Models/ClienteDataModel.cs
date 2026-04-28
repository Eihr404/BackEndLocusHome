namespace Microservicio.Clientes.DataManagement.Models;

/// <summary>
/// Modelo de datos del Cliente para la Capa 2.
/// Lo que el sistema expone al exterior, NUNCA la Entity directa de la BD.
/// </summary>
public class ClienteDataModel
{
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
    public string? Domicilio { get; set; }
    public decimal Calificacion { get; set; }
    public int TotalReservas { get; set; }
    public int PuntosAcumulados { get; set; }
    public DateTime FechaRegistro { get; set; }
}

/// <summary>
/// Filtros para búsqueda de clientes en el sistema.
/// </summary>
public class ClienteFiltroDataModel
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public decimal? CalificacionMinima { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
