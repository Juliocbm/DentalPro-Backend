using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de roles
/// </summary>
public interface IRolService
{
    /// <summary>
    /// Obtiene todos los roles
    /// </summary>
    Task<IEnumerable<RolDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    Task<RolDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Obtiene un rol por su nombre
    /// </summary>
    Task<RolDto?> GetByNombreAsync(string nombre);
    
    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    Task<RolDto> CreateAsync(RolCreateDto rolCreateDto);
    
    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    Task<RolDto> UpdateAsync(RolUpdateDto rolUpdateDto);
    
    /// <summary>
    /// Elimina un rol por su ID
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un rol con el nombre especificado
    /// </summary>
    /// <param name="nombre">Nombre del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByNameAsync(string nombre);
    
    /// <summary>
    /// Verifica si existe un rol con el ID especificado
    /// </summary>
    /// <param name="id">ID del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByIdAsync(Guid id);
    
    /// <summary>
    /// Obtiene los permisos asignados a un rol
    /// </summary>
    /// <param name="idRol">ID del rol del que se desean obtener los permisos</param>
    /// <returns>Lista de permisos asociados al rol</returns>
    Task<IEnumerable<PermisoDto>> GetPermisosRolAsync(Guid idRol);
    
    /// <summary>
    /// Asigna permisos a un rol por sus IDs
    /// </summary>
    /// <param name="idRol">ID del rol al que se asignarán los permisos</param>
    /// <param name="permisoIds">Lista de IDs de permisos a asignar</param>
    /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
    Task<bool> AsignarPermisosRolAsync(Guid idRol, IEnumerable<Guid> permisoIds);
    
    /// <summary>
    /// Asigna permisos a un rol por sus nombres
    /// </summary>
    /// <param name="idRol">ID del rol al que se asignarán los permisos</param>
    /// <param name="permisoNombres">Lista de nombres de permisos a asignar</param>
    /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
    Task<bool> AsignarPermisosRolByNombreAsync(Guid idRol, IEnumerable<string> permisoNombres);
    
    /// <summary>
    /// Remueve permisos de un rol por sus IDs
    /// </summary>
    /// <param name="idRol">ID del rol del que se removerán los permisos</param>
    /// <param name="permisoIds">Lista de IDs de permisos a remover</param>
    /// <returns>True si se removieron correctamente, False en caso contrario</returns>
    Task<bool> RemoverPermisosRolAsync(Guid idRol, IEnumerable<Guid> permisoIds);
    
    /// <summary>
    /// Remueve permisos de un rol por sus nombres
    /// </summary>
    /// <param name="idRol">ID del rol del que se removerán los permisos</param>
    /// <param name="permisoNombres">Lista de nombres de permisos a remover</param>
    /// <returns>True si se removieron correctamente, False en caso contrario</returns>
    Task<bool> RemoverPermisosRolByNombreAsync(Guid idRol, IEnumerable<string> permisoNombres);
}
