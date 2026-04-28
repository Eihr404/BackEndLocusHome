using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Mappers;

/// <summary>
/// Mapper manual de la Capa 2.
/// Convierte Entities de BD en DataModels seguros para exponer al exterior.
/// </summary>
public static class ClienteDataMapper
{
    // Entity (BD) → DataModel (Capa 2)
    public static ClienteDataModel ToDataModel(ClienteEntity entity, UsuarioEntity usuario)
    {
        return new ClienteDataModel
        {
            ClienteId         = entity.ClienteId,
            UsuarioId         = entity.UsuarioId,
            NombreCompleto    = usuario.NombreCompleto,
            Email             = usuario.Email,
            Telefono          = entity.Telefono,
            FotoUrl           = entity.FotoUrl,
            Domicilio         = entity.Domicilio,
            Calificacion      = entity.Calificacion,
            TotalReservas     = entity.TotalReservas,
            PuntosAcumulados  = entity.PuntosAcumulados,
            FechaRegistro     = entity.FechaCreacion
        };
    }

    // Actualiza Entity desde DataModel (para UPDATE)
    public static void UpdateEntityFromModel(ClienteEntity entity, ClienteDataModel modelo)
    {
        entity.Telefono          = modelo.Telefono;
        entity.FotoUrl           = modelo.FotoUrl;
        entity.Domicilio         = modelo.Domicilio;
        entity.FechaModificacion = DateTime.UtcNow;
    }
}

/// <summary>
/// Mapper de Propiedades de BD → DataModel de Capa 2.
/// </summary>
public static class PropiedadDataMapper
{
    public static PropiedadDataModel ToDataModel(PropiedadEntity entity)
    {
        return new PropiedadDataModel
        {
            PropiedadId        = entity.PropiedadId,
            ColaboradorId      = entity.ColaboradorId,
            Nombre             = entity.Nombre,
            Descripcion        = entity.Descripcion,
            Direccion          = entity.Direccion,
            Estrellas          = entity.Estrellas ?? 0,
            CalificacionPromedio = entity.CalificacionPromedio,
            TotalResenas       = entity.TotalResenas,
            AdmiteMascotas     = entity.AdmiteMascotas,
            Estado             = entity.Estado,
        };
    }
}
