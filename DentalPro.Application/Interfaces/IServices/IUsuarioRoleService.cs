using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio de gestión de roles de usuario
/// </summary>
public interface IUsuarioRoleService
{
    /// <summary>
    /// Asigna un rol a un usuario por nombre del rol
    /// </summary>
    Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol);
    
    /// <summary>
    /// Remueve un rol de un usuario por nombre del rol
    /// </summary>
    Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol);
    
    /// <summary>
    /// Asigna un rol a un usuario por ID del rol
    /// </summary>
    Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol);
    
    /// <summary>
    /// Remueve un rol de un usuario por ID del rol
    /// </summary>
    Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol);
    
    /// <summary>
    /// Obtiene los roles de un usuario
    /// </summary>
    Task<IEnumerable<string>> GetRolesUsuarioAsync(Guid idUsuario);
    
    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    Task<bool> HasRolAsync(Guid idUsuario, string nombreRol);
}
