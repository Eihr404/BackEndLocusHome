using Alojamientos.API.Models;
using Alojamientos.API.Services;
using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V2;

[ApiController]
[Route("api/v2/[controller]")]
public class FotosController : ControllerBase
{
    private readonly IFotosService _service;

    public FotosController(IFotosService service)
    {
        _service = service;
    }

    [HttpGet("alojamiento/{alojamientoId}")]
    public async Task<IActionResult> GetByAlojamientoId(int alojamientoId)
        => Ok(await _service.GetByAlojamientoIdAsync(alojamientoId));

    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] AgregarFotoRequest request)
    {
        var result = await _service.AgregarAsync(request);
        return Created("", result);
    }

    [HttpPost("cloudinary")]
    public async Task<IActionResult> AgregarDesdeCloudinary([FromBody] SubirFotoCloudinaryRequest request, CancellationToken cancellationToken)
    {
        var cloudinaryUploadService = HttpContext.RequestServices.GetRequiredService<ICloudinaryUploadService>();
        var secureUrl = await cloudinaryUploadService.UploadImageFromUrlAsync(request.SourceUrl, cancellationToken);
        var result = await _service.AgregarAsync(new AgregarFotoRequest(
            request.AlojamientoId,
            secureUrl,
            request.Orden,
            request.Descripcion
        ));

        return Created("", result);
    }

    [HttpPost("cloudinary/archivo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> AgregarArchivoDesdeCloudinary(
        [FromForm] SubirFotoCloudinaryArchivoRequest request,
        CancellationToken cancellationToken = default)
    {
        var cloudinaryUploadService = HttpContext.RequestServices.GetRequiredService<ICloudinaryUploadService>();
        var archivo = request.Archivo;
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debes seleccionar una imagen valida." });
        }

        string secureUrl;
        try
        {
            await using var stream = archivo.OpenReadStream();
            secureUrl = await cloudinaryUploadService.UploadImageAsync(
                stream,
                archivo.FileName,
                archivo.ContentType,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { mensaje = ex.Message });
        }

        var result = await _service.AgregarAsync(new AgregarFotoRequest(
            request.AlojamientoId,
            secureUrl,
            request.Orden,
            request.Descripcion
        ));

        return Created("", result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
