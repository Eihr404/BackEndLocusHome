using Microservicio.Cliente.DatAccess.Entities.Configuracion;
using Microservicio.Clientes.Business.DTOs.Maestros;

namespace Microservicio.Clientes.Business.Mappers;

public static class MaestrosBusinessMapper
{
    public static CiudadDto ToResponse(CiudadEntity entity) => new CiudadDto
    {
        CiudadId = entity.CiudadId,
        Nombre = entity.Nombre,
        EsCapital = entity.EsCapital,
        PaisId = entity.PaisId
    };

    public static MonedaDto ToResponse(MonedaEntity entity) => new MonedaDto
    {
        MonedaId = entity.MonedaId,
        Nombre = entity.Nombre,
        Simbolo = entity.Simbolo,
        Codigo = entity.Codigo
    };

    public static TipoAlojamientoDto ToResponse(TipoAlojamientoEntity entity) => new TipoAlojamientoDto
    {
        TipoAlojamientoId = entity.TipoAlojamientoId,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion
    };

    public static InstalacionDto ToResponse(CatalogoInstalacionEntity entity) => new InstalacionDto
    {
        InstalacionId = entity.InstalacionId,
        Nombre = entity.Nombre,
        Icono = entity.Icono
    };
}
