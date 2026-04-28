using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Clientes.Business.DTOs.Habitaciones;

namespace Microservicio.Clientes.Business.Mappers;

public static class HabitacionesBusinessMapper
{
    public static HabitacionDto ToResponse(HabitacionEntity entity) => new HabitacionDto
    {
        HabitacionId = entity.HabitacionId,
        PropiedadId = entity.PropiedadId,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        CapacidadAdultos = entity.CapacidadAdultos,
        CapacidadNinos = entity.CapacidadNinos,
        NumBanos = entity.NumBanos,
        NumDormitorios = entity.NumDormitorios,
        AdmiteMascotas = entity.AdmiteMascotas,
        TieneCocina = entity.TieneCocina,
        TieneAireAcondicionado = entity.TieneAireAcondicionado,
        SuperficieM2 = entity.SuperficieM2,
        Estado = entity.Estado
    };

    public static HabitacionEntity ToDataModel(CrearHabitacionDto dto) => new HabitacionEntity
    {
        PropiedadId = dto.PropiedadId,
        Nombre = dto.Nombre,
        CapacidadAdultos = dto.CapacidadAdultos,
        NumBanos = dto.NumBanos,
        CapacidadNinos = dto.CapacidadNinos,
        NumDormitorios = dto.NumDormitorios,
        Descripcion = dto.Descripcion,
        AdmiteMascotas = dto.AdmiteMascotas,
        TieneCocina = dto.TieneCocina,
        TieneAireAcondicionado = dto.TieneAireAcondicionado,
        SuperficieM2 = dto.SuperficieM2,
        UsuarioCreacion = "System"
    };
}
