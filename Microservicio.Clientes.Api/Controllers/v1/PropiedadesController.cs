using Asp.Versioning;
using Microservicio.Clientes.Api.Models.Common;
using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/propiedades")]
[Produces("application/json")]
public class PropiedadesController : ControllerBase
{
    private readonly IPropiedadService _propiedadService;

    public PropiedadesController(IPropiedadService propiedadService) => _propiedadService = propiedadService;

    /// <summary>Busca propiedades disponibles con filtros (marketplace público).</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PropiedadTarjetaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] BuscarPropiedadRequest request)
    {
        var result = await _propiedadService.BuscarAsync(request);
        return Ok(ApiResponse<PagedResponse<PropiedadTarjetaResponse>>.Ok(result));
    }

    /// <summary>Obtiene el detalle completo de una propiedad.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PropiedadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await _propiedadService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<PropiedadResponse>.Ok(result));
    }

    /// <summary>Lista todas las propiedades de un colaborador.</summary>
    [HttpGet("colaborador/{colaboradorId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PropiedadResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorColaborador(int colaboradorId)
    {
        var result = await _propiedadService.ObtenerPorColaboradorAsync(colaboradorId);
        return Ok(ApiResponse<IReadOnlyCollection<PropiedadResponse>>.Ok(result));
    }

    /// <summary>Registra una nueva propiedad (requiere ser colaborador autenticado).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador,Colaborador")]
    [ProducesResponseType(typeof(ApiResponse<PropiedadResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearPropiedadRequest request)
    {
        var result = await _propiedadService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.PropiedadId },
            ApiResponse<PropiedadResponse>.Ok(result, "Propiedad registrada exitosamente."));
    }

    /// <summary>Cambia el estado operativo de una propiedad (Activa/Inactiva/EnMantenimiento).</summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Administrador,Colaborador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPropiedadRequest request)
    {
        request.PropiedadId = id;
        await _propiedadService.CambiarEstadoAsync(request);
        return NoContent();
    }
}
