namespace Microservicio.Clientes.Api.Models.Common;

/// <summary>Respuesta estándar de error de la API (4xx / 5xx).</summary>
public class ApiErrorResponse
{
    public bool Exitoso { get; set; } = false;
    public string Mensaje { get; set; } = string.Empty;
    public List<string> Errores { get; set; } = new();
    public int StatusCode { get; set; }
    public string? TraceId { get; set; }
}
