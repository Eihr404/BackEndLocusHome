using Alojamientos.Business.DTOs.Habitaciones;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V2;

[ApiController]
[Route("api/v2/[controller]")]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionesService _service;

    public HabitacionesController(IHabitacionesService service) => _service = service;

    [HttpGet("/api/v2/alojamientos/{alojamientoId}/habitaciones")]
    [HttpGet("alojamiento/{alojamientoId}")]
    public async Task<IActionResult> GetByAlojamientoId(int alojamientoId)
        => Ok(await _service.GetByAlojamientoIdAsync(alojamientoId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("/api/v2/alojamientos/{alojamientoId}/habitaciones")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromRoute] int? alojamientoId, [FromBody] CrearHabitacionRequest request)
    {
        if (alojamientoId.HasValue && alojamientoId.Value > 0)
        {
            request = request with { AlojamientoId = alojamientoId.Value };
        }

        var result = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.HabitacionId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarHabitacionRequest request)
    {
        await _service.ActualizarAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
