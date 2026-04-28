using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.Exceptions;

namespace Microservicio.Clientes.Business.Validators;

/// <summary>Reglas de validación de negocio para la entidad Cliente.</summary>
public static class ClienteValidator
{
    public static void Validar(CrearClienteRequest req)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(req.NombreCompleto) || req.NombreCompleto.Length < 3)
            errores.Add("El nombre completo debe tener al menos 3 caracteres.");
            
        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains("@") || !req.Email.Contains("."))
            errores.Add("El correo electrónico no tiene un formato válido (ejemplo@correo.com).");
            
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            errores.Add("La seguridad es prioridad: La contraseña debe tener al menos 8 caracteres.");
            
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }

    public static void Validar(ActualizarClienteRequest req)
    {
        if (req.ClienteId <= 0)
            throw new ValidationException(new List<string> { "Identificador de cliente no válido." });
    }
}
