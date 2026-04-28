using Microservicio.Clientes.Business.DTOs.Calificaciones;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class CalificacionesController : ControllerBase
{
    private readonly ICalificacionesService _service;

    public CalificacionesController(ICalificacionesService service)
    {
        _service = service;
    }

    [HttpGet("por-propiedad/{propiedadId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorPropiedad(int propiedadId)
    {
        var result = await _service.ObtenerPorPropiedadAsync(propiedadId);
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpPost]
    [Authorize] // Solo clientes logueados pueden calificar
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Agregar([FromBody] CrearCalificacionHotelDto dto)
    {
        // OWNER CHECK: Validar que el token pertenezca al cliente que quiere calificar
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claimId != dto.ClienteId.ToString())
        {
            return Forbid(); // Retorna 403 si intentas calificar a nombre de otro
        }

        var result = await _service.AgregarCalificacionAsync(dto);
        return Ok(new { exitoso = true, mensaje = "Calificación agregada.", datos = result });
    }
}
