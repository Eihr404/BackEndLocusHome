namespace GraphQLGateway.Models;

public sealed record LoginSession(
    string Token,
    string Rol,
    string NombreCompleto,
    int UsuarioId,
    int? ClienteId);
