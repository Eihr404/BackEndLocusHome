using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.Exceptions;

namespace Microservicio.Clientes.Business.Validators;

public static class PropiedadValidator
{
    public static void Validar(CrearPropiedadRequest req)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Nombre))
            errores.Add("El nombre de la propiedad es obligatorio.");
        if (req.Estrellas < 1 || req.Estrellas > 5)
            errores.Add("Las estrellas deben estar entre 1 y 5.");
        if (req.CiudadId <= 0)
            errores.Add("La ciudad es obligatoria.");
        if (req.ColaboradorId <= 0)
            errores.Add("El colaborador es obligatorio.");
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }
}
