namespace GraphQLGateway.Models;

public sealed record RegistrarClienteInput(
    string Email,
    string Password,
    string NombreCompleto,
    string Cedula,
    string Telefono,
    string Domicilio);
