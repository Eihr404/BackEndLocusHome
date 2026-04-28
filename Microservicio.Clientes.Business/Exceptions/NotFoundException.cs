namespace Microservicio.Clientes.Business.Exceptions;

/// <summary>Recurso no encontrado (HTTP 404).</summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} con id '{id}' no fue encontrado.") { }
}
