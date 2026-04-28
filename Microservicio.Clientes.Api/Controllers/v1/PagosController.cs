using Microservicio.Clientes.Business.DTOs.Pagos;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Microservicio.Clientes.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IPagosService _service;
    private readonly IMemoryCache _cache;

    public PagosController(IPagosService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpGet("por-reserva/{reservaId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorReserva(int reservaId)
    {
        var result = await _service.ObtenerPagosPorReservaAsync(reservaId);
        return Ok(new { exitoso = true, datos = result });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProcesarPago([FromBody] ProcesarPagoDto dto, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            if (_cache.TryGetValue(idempotencyKey, out _))
            {
                return Conflict(new { exitoso = false, mensaje = "Pago duplicado detectado. El pago ya fue procesado." });
            }
            
            // Guardar en cache por 24 horas para evitar repeticiones
            _cache.Set(idempotencyKey, true, TimeSpan.FromHours(24));
        }

        var result = await _service.ProcesarPagoAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorReserva), new { reservaId = result.ReservaId }, new { exitoso = true, mensaje = "Pago procesado existosamente.", datos = result });
    }
}
