using DentalPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Interfaz para el repositorio de relaciones entre roles y permisos
    /// </summary>
    public interface IRolPermisoRepository : IGenericRepository<RolPermiso>
    {
        /// <summary>
        /// Obtiene todas las relaciones rol-permiso
        /// </summary>
        /// <returns>Lista de relaciones rol-permiso</returns>
        Task<IEnumerable<RolPermiso>> GetAllAsync();

        /// <summary>
        /// Obtiene una relación rol-permiso específica por su ID
        /// </summary>
        /// <param name="idRolPermiso">ID de la relación rol-permiso</param>
        /// <returns>Relación rol-permiso encontrada o null</returns>
        Task<RolPermiso> GetByIdAsync(Guid idRolPermiso);

        /// <summary>
        /// Obtiene todas las relaciones rol-permiso para un rol específico
        /// </summary>
        /// <param name="idRol">ID del rol</param>
        /// <returns>Lista de relaciones rol-permiso</returns>
        Task<IEnumerable<RolPermiso>> GetByRolIdAsync(Guid idRol);

        /// <summary>
        /// Obtiene todas las relaciones rol-permiso para un permiso específico
        /// </summary>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>Lista de relaciones rol-permiso</returns>
        Task<IEnumerable<RolPermiso>> GetByPermisoIdAsync(Guid idPermiso);

        /// <summary>
        /// Verifica si existe una relación entre un rol y un permiso específicos
        /// </summary>
        /// <param name="idRol">ID del rol</param>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>True si existe la relación, False si no</returns>
        Task<bool> ExistsAsync(Guid idRol, Guid idPermiso);

        /// <summary>
        /// Agrega una nueva relación rol-permiso
        /// </summary>
        /// <param name="rolPermiso">Relación rol-permiso a agregar</param>
        /// <returns>Relación rol-permiso agregada</returns>
        Task<RolPermiso> AddAsync(RolPermiso rolPermiso);
        
        /// <summary>
        /// Agrega múltiples relaciones rol-permiso en una sola operación
        /// </summary>
        /// <param name="rolPermisos">Lista de relaciones rol-permiso a agregar</param>
        /// <returns>True si se agregaron correctamente, False si no</returns>
        Task<bool> AddRangeAsync(IEnumerable<RolPermiso> rolPermisos);

        /// <summary>
        /// Elimina una relación rol-permiso por ID de rol y permiso
        /// </summary>
        /// <param name="idRol">ID del rol</param>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>True si se eliminó correctamente, False si no</returns>
        Task<bool> DeleteByRolAndPermisoAsync(Guid idRol, Guid idPermiso);

        /// <summary>
        /// Elimina todas las relaciones rol-permiso para un rol específico
        /// </summary>
        /// <param name="idRol">ID del rol</param>
        /// <returns>True si se eliminaron correctamente, False si no</returns>
        Task<bool> DeleteAllByRolAsync(Guid idRol);
    }
}
