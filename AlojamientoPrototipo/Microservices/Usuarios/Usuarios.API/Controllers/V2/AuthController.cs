using Microsoft.AspNetCore.Mvc;
using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;

namespace Usuarios.API.Controllers.V2;

[ApiController]
[Route("api/v2/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales invalidas",
                diagnostico = result.FailureReason
            });
        }

        return Ok(result.Session);
    }
}
