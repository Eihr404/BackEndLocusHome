using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Clientes.Business.DTOs.Colaboradores;

namespace Microservicio.Clientes.Business.Mappers;

public static class ColaboradoresBusinessMapper
{
    public static ColaboradorDto ToResponse(ColaboradorEntity entity) => new ColaboradorDto
    {
        ColaboradorId = entity.ColaboradorId,
        UsuarioId = entity.UsuarioId,
        NombreEmpresa = entity.NombreEmpresa,
        Telefono = entity.Telefono,
        CuentaBancaria = entity.CuentaBancaria,
        Verificado = entity.Verificado,
        PuntosAcumulados = entity.PuntosAcumulados
    };

    public static ColaboradorEntity ToDataModel(CrearColaboradorDto dto) => new ColaboradorEntity
    {
        UsuarioId = dto.UsuarioId,
        NombreEmpresa = dto.NombreEmpresa,
        Telefono = dto.Telefono
    };
}
