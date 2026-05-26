using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Alojamientos.API.Services;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class FotosController : ControllerBase
{
    private readonly IFotosService _service;
    private readonly ICloudinaryUploadService _cloudinaryUploadService;

    public FotosController(IFotosService service, ICloudinaryUploadService cloudinaryUploadService)
    {
        _service = service;
        _cloudinaryUploadService = cloudinaryUploadService;
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
        var secureUrl = await _cloudinaryUploadService.UploadImageFromUrlAsync(request.SourceUrl, cancellationToken);
        var result = await _service.AgregarAsync(new AgregarFotoRequest(
            request.AlojamientoId,
            secureUrl,
            request.Orden,
            request.Descripcion
        ));

        return Created("", result);
    }

    [HttpPost("cloudinary/archivo")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> AgregarArchivoDesdeCloudinary(
        [FromForm] int alojamientoId,
        [FromForm] IFormFile archivo,
        [FromForm] int orden = 0,
        [FromForm] string? descripcion = null,
        CancellationToken cancellationToken = default)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debes seleccionar una imagen valida." });
        }

        await using var stream = archivo.OpenReadStream();
        var secureUrl = await _cloudinaryUploadService.UploadImageAsync(
            stream,
            archivo.FileName,
            archivo.ContentType,
            cancellationToken);

        var result = await _service.AgregarAsync(new AgregarFotoRequest(
            alojamientoId,
            secureUrl,
            orden,
            descripcion
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
