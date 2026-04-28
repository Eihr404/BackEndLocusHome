using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.Business.Mappers;

/// <summary>Traduce entre DTOs y DataModels para la entidad Cliente.</summary>
public static class ClienteBusinessMapper
{
    public static ClienteDataModel ToDataModel(CrearClienteRequest req) => new()
    {
        NombreCompleto = req.NombreCompleto,
        Email          = req.Email,
        Password       = req.Password,
        Telefono       = req.Telefono,
        Domicilio      = req.Domicilio,
        FotoUrl        = req.FotoUrl
    };

    public static ClienteResponse ToResponse(ClienteDataModel m) => new()
    {
        ClienteId        = m.ClienteId,
        NombreCompleto   = m.NombreCompleto,
        Email            = m.Email,
        Telefono         = m.Telefono,
        FotoUrl          = m.FotoUrl,
        Domicilio        = m.Domicilio,
        Calificacion     = m.Calificacion,
        TotalReservas    = m.TotalReservas,
        PuntosAcumulados = m.PuntosAcumulados,
        FechaRegistro    = m.FechaRegistro
    };

    public static ClienteResumenResponse ToResumen(ClienteDataModel m) => new()
    {
        ClienteId      = m.ClienteId,
        NombreCompleto = m.NombreCompleto,
        Email          = m.Email,
        Calificacion   = m.Calificacion,
        TotalReservas  = m.TotalReservas
    };

    public static PagedResponse<ClienteResumenResponse> ToPagedResponse(DataPagedResult<ClienteDataModel> paged) => new()
    {
        Items        = paged.Items.Select(ToResumen).ToList().AsReadOnly(),
        PageNumber   = paged.PageNumber,
        PageSize     = paged.PageSize,
        TotalRecords = paged.TotalRecords
    };
}
