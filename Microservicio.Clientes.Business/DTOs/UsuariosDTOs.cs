namespace Microservicio.Clientes.Business.DTOs.Usuarios;

// ── Request DTOs ─────────────────────────────────────
public record RegistrarClienteRequest(
    string Email,
    string Password,
    string NombreCompleto,
    string Cedula,
    string Telefono,
    string Domicilio
);

public record LoginRequest(
    string Email,
    string Password
);

// ── Response DTOs ────────────────────────────────────
public record UsuarioResponse(
    int UsuarioId,
    string Rol,
    string Email,
    string NombreCompleto,
    bool Estado,
    DateTime FechaCreacion
);

public record ClienteResponse(
    int ClienteId,
    int? UsuarioId,
    string Cedula,
    string? FotoUrl,
    string? Telefono,
    string? Domicilio,
    string Email,
    int TotalReservas,
    DateTime FechaCreacion,
    UsuarioResponse? Usuario
);

public record LoginResponse(
    string Token,
    UsuarioResponse Usuario
);
