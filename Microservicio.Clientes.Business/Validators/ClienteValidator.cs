using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.Exceptions;

namespace Microservicio.Clientes.Business.Validators;

/// <summary>Reglas de validación de negocio para la entidad Cliente.</summary>
public static class ClienteValidator
{
    public static void Validar(CrearClienteRequest req)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(req.NombreCompleto))
            errores.Add("El nombre completo es obligatorio.");
        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
            errores.Add("El email es inválido o está vacío.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            errores.Add("La contraseña debe tener al menos 8 caracteres.");
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }

    public static void Validar(ActualizarClienteRequest req)
    {
        if (req.ClienteId <= 0)
            throw new ValidationException(["El ClienteId es inválido."]);
    }
}
