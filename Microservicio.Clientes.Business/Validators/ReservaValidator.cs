using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.Business.Exceptions;

namespace Microservicio.Clientes.Business.Validators;

public static class ReservaValidator
{
    public static void Validar(CrearReservaRequest req)
    {
        var errores = new List<string>();
        if (req.FechaCheckOut <= req.FechaCheckIn)
            errores.Add("La fecha de check-out debe ser posterior a la de check-in.");
        if (req.FechaCheckIn.Date < DateTime.Today)
            errores.Add("La fecha de check-in no puede ser en el pasado.");
        if (req.NumAdultos < 1)
            errores.Add("Debe haber al menos 1 adulto en la reserva.");
        if (!req.HabitacionIds.Any())
            errores.Add("Debe seleccionar al menos una habitación.");
        if (req.ClienteId <= 0 || req.PropiedadId <= 0)
            errores.Add("ClienteId y PropiedadId son obligatorios.");
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }
}
