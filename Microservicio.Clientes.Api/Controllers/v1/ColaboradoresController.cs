using Microservicio.Clientes.Business.DTOs.Colaboradores;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Administrador")] // Solo el admin o dueños pueden ver esto inicialmente
public class ColaboradoresController : ControllerBase
{
    private readonly IColaboradoresService _service;

    public ColaboradoresController(IColaboradoresService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos()
    {
        var result = await _service.ObtenerTodosAsync();
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await _service.ObtenerPorIdAsync(id);
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearColaboradorDto dto)
    {
        var result = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.ColaboradorId }, new { exitoso = true, mensaje = "Colaborador creado.", datos = result });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarColaboradorDto dto)
    {
        var result = await _service.ActualizarAsync(id, dto);
        return Ok(new { exitoso = true, mensaje = "Colaborador actualizado.", datos = result });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return Ok(new { exitoso = true, mensaje = "Colaborador eliminado." });
    }
}
