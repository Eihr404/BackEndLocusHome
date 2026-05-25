using Microsoft.Extensions.Logging;
using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;
using Usuarios.DataManagement.Interfaces;

namespace Usuarios.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUsuariosDataService _usuariosDataService;
    private readonly IClientesDataService _clientesDataService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuariosDataService usuariosDataService,
        IClientesDataService clientesDataService,
        ILogger<AuthService> logger)
    {
        _usuariosDataService = usuariosDataService;
        _clientesDataService = clientesDataService;
        _logger = logger;
    }

    public async Task<LoginAttemptResult> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        _logger.LogInformation("Intento de login para {Email}", email);

        var usuario = await _usuariosDataService.GetByEmailAsync(email);
        if (usuario is null)
        {
            _logger.LogWarning("Login rechazado para {Email}: usuario_no_encontrado", email);
            return new LoginAttemptResult(false, FailureReason: "usuario_no_encontrado");
        }

        if (!usuario.Estado)
        {
            _logger.LogWarning("Login rechazado para {Email}: usuario_inactivo", email);
            return new LoginAttemptResult(false, FailureReason: "usuario_inactivo");
        }

        var passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValida)
        {
            _logger.LogWarning("Login rechazado para {Email}: password_invalida", email);
            return new LoginAttemptResult(false, FailureReason: "password_invalida");
        }

        var cliente = await _clientesDataService.GetByUsuarioIdAsync(usuario.UsuarioId);
        _logger.LogInformation(
            "Login exitoso para {Email}. UsuarioId={UsuarioId}, ClienteId={ClienteId}, Rol={Rol}",
            email,
            usuario.UsuarioId,
            cliente?.ClienteId,
            usuario.Rol);

        return new LoginAttemptResult(
            true,
            new LoginResponse(
                Token: $"session-{usuario.UsuarioId}-{Guid.NewGuid():N}",
                Rol: usuario.Rol,
                NombreCompleto: usuario.NombreCompleto,
                UsuarioId: usuario.UsuarioId,
                ClienteId: cliente?.ClienteId
            ));
    }
}
