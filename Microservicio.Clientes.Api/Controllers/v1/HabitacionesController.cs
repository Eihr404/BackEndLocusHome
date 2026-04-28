using Microservicio.Clientes.Business.DTOs.Habitaciones;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionesService _service;

    public HabitacionesController(IHabitacionesService service)
    {
        _service = service;
    }

    [HttpGet("por-propiedad/{propiedadId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorPropiedad(int propiedadId)
    {
        var result = await _service.ObtenerPorPropiedadAsync(propiedadId);
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await _service.ObtenerPorIdAsync(id);
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Colaborador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearHabitacionDto dto)
    {
        var result = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.HabitacionId }, new { exitoso = true, mensaje = "Habitación creada.", datos = result });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Colaborador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarHabitacionDto dto)
    {
        var result = await _service.ActualizarAsync(id, dto);
        return Ok(new { exitoso = true, mensaje = "Habitación actualizada.", datos = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Colaborador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return Ok(new { exitoso = true, mensaje = "Habitación eliminada." });
    }
}
