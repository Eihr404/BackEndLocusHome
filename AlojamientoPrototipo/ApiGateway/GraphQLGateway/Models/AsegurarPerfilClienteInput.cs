namespace GraphQLGateway.Models;

public sealed record AsegurarPerfilClienteInput(
    int UsuarioId,
    string Email,
    string NombreCompleto,
    string? Cedula,
    string? Telefono,
    string? Domicilio);
