using DentalPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Interfaz para el repositorio de permisos directamente asignados a usuarios
    /// </summary>
    public interface IUsuarioPermisoRepository : IGenericRepository<UsuarioPermiso>
    {
        /// <summary>
        /// Obtiene todos los permisos asignados directamente a los usuarios
        /// </summary>
        /// <returns>Lista de relaciones usuario-permiso</returns>
        Task<IEnumerable<UsuarioPermiso>> GetAllAsync();

        /// <summary>
        /// Obtiene una asignación de permiso a usuario específica por su ID
        /// </summary>
        /// <param name="id">ID de la relación usuario-permiso</param>
        /// <returns>Relación usuario-permiso encontrada o null</returns>
        Task<UsuarioPermiso> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las asignaciones de permisos para un usuario específico
        /// </summary>
        /// <param name="idUsuario">ID del usuario</param>
        /// <returns>Lista de relaciones usuario-permiso</returns>
        Task<IEnumerable<UsuarioPermiso>> GetByUsuarioIdAsync(Guid idUsuario);

        /// <summary>
        /// Obtiene todas las asignaciones de un permiso específico a distintos usuarios
        /// </summary>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>Lista de relaciones usuario-permiso</returns>
        Task<IEnumerable<UsuarioPermiso>> GetByPermisoIdAsync(Guid idPermiso);

        /// <summary>
        /// Verifica si existe una asignación directa de un permiso a un usuario
        /// </summary>
        /// <param name="idUsuario">ID del usuario</param>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>True si existe la asignación, False si no</returns>
        Task<bool> ExistsAsync(Guid idUsuario, Guid idPermiso);

        /// <summary>
        /// Agrega una nueva asignación de permiso a usuario
        /// </summary>
        /// <param name="usuarioPermiso">Relación usuario-permiso a agregar</param>
        /// <returns>Relación usuario-permiso agregada</returns>
        Task<UsuarioPermiso> AddAsync(UsuarioPermiso usuarioPermiso);
        
        /// <summary>
        /// Agrega múltiples asignaciones de permisos a usuario en una sola operación
        /// </summary>
        /// <param name="usuarioPermisos">Lista de relaciones usuario-permiso a agregar</param>
        /// <returns>True si se agregaron correctamente, False si no</returns>
        Task<bool> AddRangeAsync(IEnumerable<UsuarioPermiso> usuarioPermisos);

        /// <summary>
        /// Elimina una asignación de permiso a usuario por ID de usuario y permiso
        /// </summary>
        /// <param name="idUsuario">ID del usuario</param>
        /// <param name="idPermiso">ID del permiso</param>
        /// <returns>True si se eliminó correctamente, False si no</returns>
        Task<bool> DeleteByUsuarioAndPermisoAsync(Guid idUsuario, Guid idPermiso);

        /// <summary>
        /// Elimina todas las asignaciones de permisos para un usuario específico
        /// </summary>
        /// <param name="idUsuario">ID del usuario</param>
        /// <returns>True si se eliminaron correctamente, False si no</returns>
        Task<bool> DeleteAllByUsuarioAsync(Guid idUsuario);
    }
}
