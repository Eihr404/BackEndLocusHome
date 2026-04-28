using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Interfaces;

/// <summary>
/// Contrato del Servicio de Datos de Cliente.
/// Define qué operaciones puede pedir la capa Business a DataManagement.
/// </summary>
public interface IClienteDataService
{
    Task<ClienteDataModel?> ObtenerPorIdAsync(int clienteId);
    Task<ClienteDataModel?> ObtenerPorUsuarioIdAsync(int usuarioId);
    Task<DataPagedResult<ClienteDataModel>> BuscarAsync(ClienteFiltroDataModel filtro);
    Task<int> CrearAsync(ClienteDataModel modelo);
    Task ActualizarAsync(ClienteDataModel modelo);
    Task EliminarAsync(int clienteId);
}

/// <summary>
/// Contrato del Servicio de Datos de Propiedad/Alojamiento.
/// </summary>
public interface IPropiedadDataService
{
    Task<PropiedadDataModel?> ObtenerPorIdAsync(int propiedadId);
    Task<DataPagedResult<PropiedadDataModel>> BuscarAsync(PropiedadFiltroDataModel filtro);
    Task<IReadOnlyCollection<PropiedadDataModel>> ObtenerPorColaboradorAsync(int colaboradorId);
}

/// <summary>
/// Contrato del Servicio de Datos de Reservas.
/// </summary>
public interface IReservaDataService
{
    Task<ReservaDataModel?> ObtenerPorCodigoAsync(string codigo);
    Task<IReadOnlyCollection<ReservaDataModel>> ObtenerTodasAsync();
    Task<IReadOnlyCollection<ReservaDataModel>> ObtenerPorClienteAsync(int clienteId);
    Task<ReservaDataModel> CrearReservaAsync(CrearReservaDataModel modelo);
    Task CambiarEstadoAsync(int reservaId, string nuevoEstado);
}
