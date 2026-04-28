using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.Business.Mappers;

public static class PropiedadBusinessMapper
{
    public static Microservicio.Cliente.DatAccess.Entities.Core.PropiedadEntity ToEntity(CrearPropiedadRequest req) => new()
    {
        ColaboradorId  = req.ColaboradorId,
        Nombre         = req.Nombre,
        Descripcion    = req.Descripcion,
        Direccion      = req.Direccion,
        CiudadId       = req.CiudadId,
        TipoAlojamientoId = req.TipoAlojamientoId,
        Estrellas      = req.Estrellas,
        AdmiteMascotas = req.AdmiteMascotas,
        Estado         = "Activa",
        UsuarioCreacion = "System"
    };

    public static PropiedadResponse ToResponse(PropiedadDataModel m) => new()
    {
        PropiedadId          = m.PropiedadId,
        ColaboradorId        = m.ColaboradorId,
        Nombre               = m.Nombre,
        Ciudad               = m.Ciudad,
        Descripcion          = m.Descripcion,
        Direccion            = m.Direccion,
        Estrellas            = m.Estrellas,
        CalificacionPromedio = m.CalificacionPromedio,
        TotalResenas         = m.TotalResenas,
        AdmiteMascotas       = m.AdmiteMascotas,
        Estado               = m.Estado
    };

    public static PropiedadTarjetaResponse ToTarjeta(PropiedadDataModel m) => new()
    {
        PropiedadId          = m.PropiedadId,
        Nombre               = m.Nombre,
        Ciudad               = m.Ciudad,
        Estrellas            = m.Estrellas,
        CalificacionPromedio = m.CalificacionPromedio,
        AdmiteMascotas       = m.AdmiteMascotas
    };

    public static PagedResponse<PropiedadTarjetaResponse> ToPagedResponse(DataPagedResult<PropiedadDataModel> paged) => new()
    {
        Items        = paged.Items.Select(ToTarjeta).ToList().AsReadOnly(),
        PageNumber   = paged.PageNumber,
        PageSize     = paged.PageSize,
        TotalRecords = paged.TotalRecords
    };
}
