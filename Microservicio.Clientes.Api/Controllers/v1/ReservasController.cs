using Asp.Versioning;
using Microservicio.Clientes.Api.Models.Common;
using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassTransit;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reservas")]
[Authorize]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReservasController(IReservaService reservaService, IPublishEndpoint publishEndpoint)
    {
        _reservaService = reservaService;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>Lista todas las reservas del sistema (Solo Administradores).</summary>
    [HttpGet("todas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ReservaResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodas()
    {
        var result = await _reservaService.ObtenerTodasAsync();
        return Ok(ApiResponse<IReadOnlyCollection<ReservaResumenResponse>>.Ok(result));
    }

    /// <summary>Obtiene el detalle de una reserva por su código único (BK-XXXXXXXX-N).</summary>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(ApiResponse<ReservaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        var result = await _reservaService.ObtenerPorCodigoAsync(codigo);
        if (result is null)
            return NotFound(new ApiErrorResponse { Mensaje = $"Reserva '{codigo}' no encontrada.", StatusCode = 404 });
        return Ok(ApiResponse<ReservaResponse>.Ok(result));
    }

    /// <summary>Lista el historial de reservas de un cliente.</summary>
    [HttpGet("cliente/{clienteId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ReservaResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorCliente(int clienteId)
    {
        var result = await _reservaService.ObtenerPorClienteAsync(clienteId);
        return Ok(ApiResponse<IReadOnlyCollection<ReservaResumenResponse>>.Ok(result));
    }

    /// <summary>Crea una nueva reserva y devuelve el código de confirmación.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReservaConfirmadaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearReservaRequest request)
    {
        var result = await _reservaService.CrearAsync(request);
        
        // EDA: Publicar el evento de integración al Bus
        await _publishEndpoint.Publish(new ReservaConfirmadaIntegrationEvent
        {
            CodigoReserva = result.CodigoReserva ?? string.Empty,
            ClienteId = request.ClienteId,
            PropiedadId = request.PropiedadId,
            Total = result.Total
        });

        return CreatedAtAction(nameof(ObtenerPorCodigo), new { codigo = result.CodigoReserva },
            ApiResponse<ReservaConfirmadaResponse>.Ok(result, result.Mensaje));
    }

    /// <summary>Cambia el estado de una reserva (Confirmada, Completada, NoShow).</summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoReservaRequest request)
    {
        request.ReservaId = id;
        await _reservaService.CambiarEstadoAsync(request);
        return NoContent();
    }

    /// <summary>Cancela una reserva indicando el motivo (obligatorio).</summary>
    [HttpDelete("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarReservaRequest request)
    {
        request.ReservaId = id;
        await _reservaService.CancelarAsync(request);
        return NoContent();
    }
}
