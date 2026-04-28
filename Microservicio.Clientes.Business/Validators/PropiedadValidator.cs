using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.Exceptions;

namespace Microservicio.Clientes.Business.Validators;

public static class PropiedadValidator
{
    public static void Validar(CrearPropiedadRequest req)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(req.Nombre) || req.Nombre.Length < 5)
            errores.Add("El nombre de la propiedad debe ser descriptivo (mínimo 5 caracteres).");
            
        if (req.Estrellas < 1 || req.Estrellas > 5)
            errores.Add("La calificación debe estar entre 1 y 5 estrellas.");
            
        if (req.CiudadId <= 0)
            errores.Add("Debes seleccionar una ciudad válida.");
            
        if (req.ColaboradorId <= 0)
            errores.Add("Toda propiedad debe estar vinculada a un colaborador responsable.");
            
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }
}
