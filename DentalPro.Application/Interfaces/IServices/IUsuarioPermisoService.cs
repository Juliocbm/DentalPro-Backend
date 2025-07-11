using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Application.DTOs.Permiso;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio de gestión de permisos directos de usuario
/// </summary>
public interface IUsuarioPermisoService
{
    /// <summary>
    /// Asigna un permiso a un usuario por nombre del permiso
    /// </summary>
    Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso);
    
    /// <summary>
    /// Asigna un permiso a un usuario por ID del permiso
    /// </summary>
    Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso);
    
    /// <summary>
    /// Remueve un permiso de un usuario por nombre del permiso
    /// </summary>
    Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso);
    
    /// <summary>
    /// Remueve un permiso de un usuario por ID del permiso
    /// </summary>
    Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso);
    
    /// <summary>
    /// Obtiene los nombres de permisos directos de un usuario
    /// </summary>
    Task<IEnumerable<string>> GetPermisosAsync(Guid idUsuario);
    
    /// <summary>
    /// Obtiene información detallada de permisos directos de un usuario
    /// </summary>
    Task<IEnumerable<PermisoDto>> GetPermisosUsuarioAsync(Guid idUsuario);
    
    /// <summary>
    /// Asigna múltiples permisos a un usuario por sus IDs
    /// </summary>
    Task<bool> AsignarPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos);
    
    /// <summary>
    /// Asigna múltiples permisos a un usuario por sus nombres
    /// </summary>
    Task<bool> AsignarPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos);
    
    /// <summary>
    /// Remueve múltiples permisos de un usuario por sus IDs
    /// </summary>
    Task<bool> RemoverPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos);
    
    /// <summary>
    /// Remueve múltiples permisos de un usuario por sus nombres
    /// </summary>
    Task<bool> RemoverPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos);
    
    /// <summary>
    /// Verifica si un usuario tiene un permiso específico directamente asignado
    /// </summary>
    Task<bool> HasPermisoDirectoAsync(Guid idUsuario, string nombrePermiso);
}
