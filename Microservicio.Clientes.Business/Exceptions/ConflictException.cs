namespace Microservicio.Clientes.Business.Exceptions;

/// <summary>Conflicto de datos: duplicados o solapamientos (HTTP 409).</summary>
public class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message) { }
}
