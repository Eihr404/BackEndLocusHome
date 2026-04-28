namespace Microservicio.Clientes.Business.Exceptions;

/// <summary>Excepción base para errores de lógica de negocio (HTTP 400).</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
