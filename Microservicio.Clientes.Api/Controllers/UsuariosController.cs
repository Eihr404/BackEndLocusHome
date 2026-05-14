using Microservicio.Clientes.Business.DTOs.Usuarios;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    // ── Endpoints de Clientes ────────────────────────
    [HttpGet("clientes")]
    public async Task<IActionResult> GetAllClientes()
        => Ok(await _service.GetAllClientesAsync());

    [HttpGet("clientes/{clienteId}")]
    public async Task<IActionResult> GetClienteById(int clienteId)
    {
        var result = await _service.GetClienteByIdAsync(clienteId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("clientes/cedula/{cedula}")]
    public async Task<IActionResult> GetClienteByCedula(string cedula)
    {
        var result = await _service.GetClienteByCedulaAsync(cedula);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Registra un nuevo Cliente usando el SP sp_registrar_cliente (crea Usuario + Cliente atómicamente).
    /// </summary>
    [HttpPost("clientes/registrar")]
    public async Task<IActionResult> RegistrarCliente([FromBody] RegistrarClienteRequest request)
    {
        await _service.RegistrarClienteAsync(request);
        return Created("", new { mensaje = "Cliente registrado exitosamente" });
    }
}
