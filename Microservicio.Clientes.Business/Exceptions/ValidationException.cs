namespace Microservicio.Clientes.Business.Exceptions;

/// <summary>Error de validación de datos (HTTP 422). Acumula todos los errores.</summary>
public class ValidationException : BusinessException
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(IEnumerable<string> errors)
        : base("Existen errores de validación.")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
