using DentalPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Interfaz para el repositorio de permisos
    /// </summary>
    public interface IPermisoRepository : IGenericRepository<Permiso>
    {
        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del permiso</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permiso> GetByNombreAsync(string nombre);

        /// <summary>
        /// Obtiene los permisos de un rol específico
        /// </summary>
        /// <param name="rolId">ID del rol</param>
        /// <returns>Lista de permisos asociados al rol</returns>
        Task<IEnumerable<Permiso>> GetByRolIdAsync(Guid rolId);

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="rolId">ID del rol</param>
        /// <param name="permisoIds">Lista de IDs de permisos a asignar</param>
        /// <returns>True si se asignaron correctamente, False si no</returns>
        Task<bool> AsignarPermisosARolAsync(Guid rolId, IEnumerable<Guid> permisoIds);

        /// <summary>
        /// Remueve permisos de un rol
        /// </summary>
        /// <param name="rolId">ID del rol</param>
        /// <param name="permisoIds">Lista de IDs de permisos a remover</param>
        /// <returns>True si se removieron correctamente, False si no</returns>
        Task<bool> RemoverPermisosDeRolAsync(Guid rolId, IEnumerable<Guid> permisoIds);
    }
}
