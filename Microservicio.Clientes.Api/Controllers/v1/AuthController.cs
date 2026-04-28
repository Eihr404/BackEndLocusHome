using Asp.Versioning;
using Microservicio.Clientes.Api.Models.Common;
using Microservicio.Clientes.Business.DTOs.Auth;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IAuthService authService, IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _unitOfWork  = unitOfWork;
    }

    /// <summary>Inicia sesión y devuelve un JWT válido por 8 horas.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Sesión iniciada correctamente."));
    }

    /// <summary>Lista todos los usuarios del sistema con su rol (Solo Administradores).</summary>
    [HttpGet("usuarios")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolDict = roles.ToDictionary(r => r.RolId, r => r.Nombre ?? "Sin Rol");

        var lista = usuarios
            .Where(u => !u.EliminadoLogico)
            .OrderByDescending(u => u.FechaCreacion)
            .Select(u => new
            {
                u.UsuarioId,
                u.NombreCompleto,
                u.Email,
                u.RolId,
                RolNombre = rolDict.TryGetValue(u.RolId, out var rn) ? rn : "Sin Rol",
                u.Estado,
                u.EmailVerificado,
                u.UltimoAcceso,
                u.FechaCreacion
            }).ToList();

        return Ok(new { exitoso = true, datos = lista });
    }

    /// <summary>Lista todos los roles disponibles en el sistema.</summary>
    [HttpGet("roles")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRoles()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var lista = roles
            .Where(r => r.Estado && !r.EliminadoLogico)
            .Select(r => new { r.RolId, r.Nombre, r.Descripcion })
            .ToList();
        return Ok(new { exitoso = true, datos = lista });
    }

    /// <summary>Cambia el rol de un usuario (Solo Administradores).</summary>
    [HttpPatch("usuarios/{id}/rol")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null)
            return NotFound(new { exitoso = false, mensaje = "Usuario no encontrado." });

        usuario.RolId = request.RolId;
        usuario.FechaModificacion = DateTime.UtcNow;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { exitoso = true, mensaje = $"Rol del usuario actualizado a RolId={request.RolId}." });
    }

    /// <summary>Activa o desactiva una cuenta de usuario (Solo Administradores).</summary>
    [HttpPatch("usuarios/{id}/estado")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoUsuarioRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null)
            return NotFound(new { exitoso = false, mensaje = "Usuario no encontrado." });

        usuario.Estado = request.Activo;
        usuario.FechaModificacion = DateTime.UtcNow;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { exitoso = true, mensaje = $"Cuenta {(request.Activo ? "activada" : "suspendida")} exitosamente." });
    }
}

// ── DTOs auxiliares para los nuevos endpoints ──
public class CambiarRolRequest
{
    public int RolId { get; set; }
}

public class CambiarEstadoUsuarioRequest
{
    public bool Activo { get; set; }
}
