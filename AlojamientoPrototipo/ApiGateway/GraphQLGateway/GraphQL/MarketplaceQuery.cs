using GraphQLGateway.Clients;
using GraphQLGateway.Models;
using HotChocolate.Types;

namespace GraphQLGateway.GraphQL;

[ExtendObjectType<Query>]
public sealed class MarketplaceQuery
{
    public async Task<IReadOnlyList<AlojamientoCardView>> GetMarketplaceCatalogAsync(
        string? ciudad,
        string? ubicacion,
        DateOnly? fechaCheckIn,
        DateOnly? fechaCheckOut,
        int? numAdultos,
        int? numNinos,
        bool? admiteMascotas,
        bool? tienePiscina,
        bool? tieneParqueadero,
        [Service] AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
    {
        if (fechaCheckIn.HasValue && fechaCheckOut.HasValue && fechaCheckOut.Value <= fechaCheckIn.Value)
        {
            return [];
        }

        var alojamientos = await alojamientosClient.GetAlojamientosAsync(cancellationToken);
        var filtrados = alojamientos.Where(a =>
            MatchesLocation(a, ubicacion, ciudad) &&
            (!admiteMascotas.HasValue || a.AdmiteMascotas == admiteMascotas.Value) &&
            (!tienePiscina.HasValue || a.TienePiscina == tienePiscina.Value) &&
            (!tieneParqueadero.HasValue || a.TieneParqueadero == tieneParqueadero.Value))
            .ToList();

        var cards = await Task.WhenAll(
            filtrados.Select(alojamiento => BuildCatalogCardAsync(
                alojamiento,
                fechaCheckIn,
                fechaCheckOut,
                numAdultos,
                numNinos,
                alojamientosClient,
                cancellationToken)));

        return cards
            .Where(card => card is not null)
            .Select(card => card!)
            .ToList();
    }

    public async Task<AlojamientoDetalleView?> GetMarketplaceAlojamientoDetalleAsync(
        int alojamientoId,
        [Service] AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
    {
        var alojamiento = await alojamientosClient.GetAlojamientoByIdAsync(alojamientoId, cancellationToken);
        if (alojamiento is null)
        {
            return null;
        }

        var habitacionesTask = alojamientosClient.GetHabitacionesByAlojamientoIdAsync(alojamientoId, cancellationToken);
        var fotosTask = alojamientosClient.GetFotosByAlojamientoIdAsync(alojamientoId, cancellationToken);
        await Task.WhenAll(habitacionesTask, fotosTask);

        var habitaciones = await habitacionesTask;
        var fotos = (await fotosTask).OrderBy(f => f.Orden).ToList();

        return new AlojamientoDetalleView(
            alojamiento,
            habitaciones,
            fotos,
            habitaciones.Count == 0 ? null : habitaciones.Min(h => h.PrecioNoche),
            habitaciones.Count == 0 ? null : habitaciones.Max(h => h.PrecioNoche));
    }

    public async Task<ClienteReservasView> GetMarketplaceClienteReservasAsync(
        int clienteId,
        [Service] ReservasClient reservasClient,
        [Service] AlojamientosClient alojamientosClient,
        [Service] FacturacionClient facturacionClient,
        CancellationToken cancellationToken)
    {
        var reservas = await reservasClient.GetReservasResumenByClienteAsync(clienteId, cancellationToken);

        var items = new List<ReservaClienteView>(reservas.Count);
        foreach (var reserva in reservas)
        {
            Alojamiento? alojamiento = null;
            FacturaResumen? factura = null;

            try
            {
                alojamiento = await alojamientosClient.GetAlojamientoByIdAsync(reserva.AlojamientoId, cancellationToken);
            }
            catch
            {
                alojamiento = null;
            }

            try
            {
                factura = await facturacionClient.GetFacturaResumenByReservaIdAsync(reserva.ReservaId, cancellationToken);
            }
            catch
            {
                factura = null;
            }

            items.Add(new ReservaClienteView(
                reserva.ReservaId,
                reserva.CodigoReserva,
                reserva.AlojamientoId,
                alojamiento?.Nombre ?? $"Alojamiento {reserva.AlojamientoId}",
                $"Cliente {reserva.ClienteId}",
                reserva.FechaCheckIn.ToString("yyyy-MM-dd"),
                reserva.FechaCheckOut.ToString("yyyy-MM-dd"),
                reserva.Estado,
                reserva.Total,
                "USD",
                factura));
        }

        return new ClienteReservasView(clienteId, items.OrderByDescending(i => i.FechaEntrada).ToList());
    }

    public Task<IReadOnlyList<TipoAlojamiento>> GetMarketplaceTiposAlojamientoAsync(
        [Service] AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
        => alojamientosClient.GetTiposAlojamientoAsync(cancellationToken);

    public Task<IReadOnlyList<MetodoPago>> GetMarketplaceMetodosPagoAsync(
        [Service] FacturacionClient facturacionClient,
        CancellationToken cancellationToken)
        => facturacionClient.GetMetodosPagoAsync(cancellationToken);

    public Task<IReadOnlyList<CalendarioDisponibilidad>> GetMarketplaceDisponibilidadHabitacionAsync(
        int habitacionId,
        int mes,
        int anio,
        [Service] AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
        => alojamientosClient.GetDisponibilidadByHabitacionAsync(habitacionId, mes, anio, cancellationToken);

    private static bool MatchesLocation(Alojamiento alojamiento, string? ubicacion, string? ciudad)
    {
        var term = !string.IsNullOrWhiteSpace(ubicacion) ? ubicacion : ciudad;
        if (string.IsNullOrWhiteSpace(term))
        {
            return true;
        }

        return ContainsIgnoreCase(alojamiento.Ciudad, term) ||
            ContainsIgnoreCase(alojamiento.Direccion, term) ||
            ContainsIgnoreCase(alojamiento.Nombre, term);
    }

    private static bool ContainsIgnoreCase(string? source, string value)
        => !string.IsNullOrWhiteSpace(source) &&
           source.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesCapacity(Habitacion habitacion, int? numAdultos, int? numNinos)
        => (!numAdultos.HasValue || habitacion.CapacidadAdultos >= numAdultos.Value) &&
           (!numNinos.HasValue || habitacion.CapacidadNinos >= numNinos.Value);

    private static IEnumerable<(int Year, int Month)> GetMonthsInRange(DateOnly fechaCheckIn, DateOnly fechaCheckOut)
    {
        var current = new DateOnly(fechaCheckIn.Year, fechaCheckIn.Month, 1);
        var limit = new DateOnly(fechaCheckOut.Year, fechaCheckOut.Month, 1);

        while (current <= limit)
        {
            yield return (current.Year, current.Month);
            current = current.AddMonths(1);
        }
    }

    private static async Task<AlojamientoCardView?> BuildCatalogCardAsync(
        Alojamiento alojamiento,
        DateOnly? fechaCheckIn,
        DateOnly? fechaCheckOut,
        int? numAdultos,
        int? numNinos,
        AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
    {
        var habitacionesTask = alojamientosClient.GetHabitacionesByAlojamientoIdAsync(alojamiento.AlojamientoId, cancellationToken);
        var fotosTask = alojamientosClient.GetFotosByAlojamientoIdAsync(alojamiento.AlojamientoId, cancellationToken);

        await Task.WhenAll(habitacionesTask, fotosTask);

        var habitaciones = await habitacionesTask;
        var fotos = await fotosTask;
        var fotoPrincipal = fotos.OrderBy(f => f.Orden).FirstOrDefault();

        var habitacionesFiltradas = habitaciones
            .Where(h => MatchesCapacity(h, numAdultos, numNinos))
            .ToList();

        if ((numAdultos.HasValue || numNinos.HasValue) && habitacionesFiltradas.Count == 0)
        {
            return null;
        }

        if (fechaCheckIn.HasValue && fechaCheckOut.HasValue)
        {
            var habitacionesDisponibles = await Task.WhenAll(
                habitacionesFiltradas.Select(async habitacion => new
                {
                    Habitacion = habitacion,
                    Disponible = await IsRoomAvailableAsync(
                        habitacion.HabitacionId,
                        fechaCheckIn.Value,
                        fechaCheckOut.Value,
                        alojamientosClient,
                        cancellationToken)
                }));

            habitacionesFiltradas = habitacionesDisponibles
                .Where(item => item.Disponible)
                .Select(item => item.Habitacion)
                .ToList();

            if (habitacionesFiltradas.Count == 0)
            {
                return null;
            }
        }

        var habitacionesBase = habitacionesFiltradas.Count == 0 ? habitaciones : habitacionesFiltradas;

        return new AlojamientoCardView(
            alojamiento.AlojamientoId,
            alojamiento.SocioId,
            alojamiento.TipoAlojamientoId,
            alojamiento.Nombre,
            alojamiento.TipoAlojamientoNombre,
            alojamiento.Ciudad ?? string.Empty,
            alojamiento.Direccion,
            habitacionesBase.Count == 0 ? 0 : habitacionesBase.Min(h => h.PrecioNoche),
            "USD",
            alojamiento.Estrellas ?? 0,
            fotoPrincipal?.Url,
            alojamiento.AdmiteMascotas,
            alojamiento.TienePiscina,
            alojamiento.TieneParqueadero,
            string.Equals(alojamiento.Estado, "Activo", StringComparison.OrdinalIgnoreCase) && habitacionesBase.Count > 0,
            alojamiento.Descripcion,
            alojamiento.Estado);
    }

    private static async Task<bool> IsRoomAvailableAsync(
        int habitacionId,
        DateOnly fechaCheckIn,
        DateOnly fechaCheckOut,
        AlojamientosClient alojamientosClient,
        CancellationToken cancellationToken)
    {
        var monthSegments = GetMonthsInRange(fechaCheckIn, fechaCheckOut).ToList();
        var availability = await Task.WhenAll(
            monthSegments.Select(month => alojamientosClient.GetDisponibilidadByHabitacionAsync(
                habitacionId,
                month.Month,
                month.Year,
                cancellationToken)));

        var blockedDates = availability
            .SelectMany(segment => segment)
            .Select(item => item.Fecha)
            .ToHashSet();

        for (var day = fechaCheckIn; day <= fechaCheckOut; day = day.AddDays(1))
        {
            if (blockedDates.Contains(day))
            {
                return false;
            }
        }

        return true;
    }
}
