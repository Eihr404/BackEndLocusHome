namespace Microservicio.Clientes.Api.Models.Common;

/// <summary>Envelope estándar de respuesta exitosa de la API.</summary>
public class ApiResponse<T>
{
    public bool Exitoso { get; set; } = true;
    public string? Mensaje { get; set; }
    public T? Datos { get; set; }

    public static ApiResponse<T> Ok(T datos, string? mensaje = null)
        => new() { Exitoso = true, Datos = datos, Mensaje = mensaje };
}
