using Microservicio.Clientes.Business.DTOs.Colaboradores;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IColaboradoresService
{
    Task<IEnumerable<ColaboradorDto>> ObtenerTodosAsync();
    Task<ColaboradorDto> ObtenerPorIdAsync(int id);
    Task<ColaboradorDto> CrearAsync(CrearColaboradorDto dto);
    Task<ColaboradorDto> ActualizarAsync(int id, ActualizarColaboradorDto dto);
    Task EliminarAsync(int id);
}
