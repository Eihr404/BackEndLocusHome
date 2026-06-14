namespace GraphQLGateway.Models;

public sealed record ReservaClienteView(
    int ReservaId,
    string CodigoReserva,
    int AlojamientoId,
    string AlojamientoNombre,
    string ClienteNombre,
    string FechaEntrada,
    string FechaSalida,
    string Estado,
    decimal Total,
    string Moneda,
    FacturaResumen? Factura);
