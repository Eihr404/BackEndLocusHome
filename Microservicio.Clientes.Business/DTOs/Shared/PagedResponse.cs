namespace Microservicio.Clientes.Business.DTOs.Shared;

/// <summary>Respuesta paginada genérica para todos los listados de la API.</summary>
public class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public long TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool TienePaginaSiguiente => PageNumber < TotalPages;
    public bool TienePaginaAnterior => PageNumber > 1;
}
