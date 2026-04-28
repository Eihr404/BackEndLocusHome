using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Protegido por JWT
public class MaestrosController : ControllerBase
{
    private readonly IMaestrosService _service;

    public MaestrosController(IMaestrosService service)
    {
        _service = service;
    }

    [HttpGet("ciudades")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCiudades()
    {
        var result = await _service.ObtenerCiudadesAsync();
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("paises")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPaises()
    {
        var result = await _service.ObtenerPaisesAsync();
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("monedas")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerMonedas()
    {
        var result = await _service.ObtenerMonedasAsync();
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("tipos-alojamiento")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTiposAlojamiento()
    {
        var result = await _service.ObtenerTiposAlojamientoAsync();
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("instalaciones")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInstalaciones()
    {
        var result = await _service.ObtenerInstalacionesAsync();
        return Ok(new { exitoso = true, datos = result });
    }
}
