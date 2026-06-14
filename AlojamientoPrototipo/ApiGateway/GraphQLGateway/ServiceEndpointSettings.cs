namespace GraphQLGateway;

public sealed class ServiceEndpointSettings
{
    public const string SectionName = "ServiceEndpoints";

    public string AlojamientosBaseUrl { get; init; } = "https://israel-alojamientos-api.onrender.com";

    public string ReservasBaseUrl { get; init; } = "https://israel-reservas-api.onrender.com";

    public string FacturacionBaseUrl { get; init; } = "https://israel-facturacion-api.onrender.com";

    public string UsuariosBaseUrl { get; init; } = "https://israel-usuarios-api.onrender.com";
}
