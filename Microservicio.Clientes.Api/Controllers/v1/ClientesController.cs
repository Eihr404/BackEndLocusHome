using Asp.Versioning;
using Microservicio.Clientes.Api.Models.Common;
using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
[Authorize]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService) => _clienteService = clienteService;

    /// <summary>Busca clientes con filtros y paginación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ClienteResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] BuscarClienteRequest request)
    {
        var result = await _clienteService.BuscarAsync(request);
        return Ok(ApiResponse<PagedResponse<ClienteResumenResponse>>.Ok(result));
    }

    /// <summary>Obtiene el perfil completo de un cliente por su ID.</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await _clienteService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<ClienteResponse>.Ok(result));
    }

    /// <summary>Registra un nuevo cliente en el sistema.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearClienteRequest request)
    {
        var result = await _clienteService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.ClienteId }, 
            ApiResponse<ClienteResponse>.Ok(result, "Cliente creado exitosamente."));
    }

    /// <summary>Actualiza los datos de contacto de un cliente.</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteRequest request)
    {
        request.ClienteId = id;
        var result = await _clienteService.ActualizarAsync(request);
        return Ok(ApiResponse<ClienteResponse>.Ok(result, "Cliente actualizado exitosamente."));
    }

    /// <summary>Elimina (baja lógica) a un cliente del sistema.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _clienteService.EliminarAsync(id);
        return NoContent();
    }
}
