using Microservicio.Cliente.DatAccess.Common;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Contracts;   // ← IRepository<T> vive aquí
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Interfaces
{
    /// <summary>Hereda IRepository para acceder al CRUD genérico (GetByIdAsync, AddAsync, etc.)</summary>
    public interface IClienteRepository : IRepository<ClienteEntity>
    {
        Task<ClienteEntity?> GetByUsuarioIdAsync(int usuarioId);
        Task<PagedResult<ClienteEntity>> GetPaginadoAsync(int pageNumber, int pageSize);
        Task<bool> ExisteAsync(int clienteId);
    }

    public interface IUsuarioRepository : IRepository<UsuarioEntity>
    {
        Task<UsuarioEntity?> GetByEmailAsync(string email);
        Task<bool> EmailExisteAsync(string email);
        Task<IEnumerable<UsuarioEntity>> GetByRolAsync(int rolId);
    }

    public interface IUsuarioRolRepository : IRepository<UsuarioRolEntity>
    {
        Task<IEnumerable<UsuarioRolEntity>> GetRolesPorUsuarioAsync(int usuarioId);
        Task AsignarRolAsync(int usuarioId, int rolId);
        Task RevocarRolAsync(int usuarioId, int rolId);
        Task<bool> TieneRolAsync(int usuarioId, int rolId);
    }
}
