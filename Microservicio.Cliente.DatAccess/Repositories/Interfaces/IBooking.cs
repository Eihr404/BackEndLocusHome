using Microservicio.Cliente.DatAccess.Common;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Cliente.DatAccess.Repositories.Contracts;   // ← IRepository<T> vive aquí
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Interfaces
{
    public interface IPropiedadRepository : IRepository<PropiedadEntity>
    {
        Task<IEnumerable<PropiedadEntity>> GetByColaboradorAsync(int colaboradorId);
        Task<PagedResult<PropiedadEntity>> BuscarAsync(int? ciudadId, bool? admiteMascotas, int pageNumber, int pageSize);
        Task<bool> ExisteAsync(int propiedadId);
    }

    public interface IReservaRepository : IRepository<ReservaEntity>
    {
        Task<ReservaEntity?> GetByCodigoAsync(string codigoReserva);
        Task<IEnumerable<ReservaEntity>> GetByClienteAsync(int clienteId);
        Task<IEnumerable<ReservaEntity>> GetByPropiedadAsync(int propiedadId);
        Task<bool> HayConflictoDisponibilidadAsync(int habitacionId, System.DateTime checkIn, System.DateTime checkOut);
        Task CambiarEstadoAsync(int reservaId, string nuevoEstado);
    }
}
