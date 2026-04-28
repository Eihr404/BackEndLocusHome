namespace Microservicio.Clientes.DataManagement.Models;

/// <summary>
/// Modelo de paginación genérico de la Capa 2.
/// IReadOnlyCollection garantiza que los resultados no sean modificados accidentalmente.
/// </summary>
public class DataPagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; } = 10;
    public long TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
