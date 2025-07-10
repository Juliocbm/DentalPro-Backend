using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para la gestión de permisos en el sistema
/// </summary>
public static class PermisosPermissions
{
    // Permisos de visualización
    /// <summary>
    /// Permiso para ver todos los permisos disponibles en el sistema
    /// </summary>
    public const string ViewAll = "permisos.view.all";
    
    /// <summary>
    /// Permiso para ver detalles de un permiso específico
    /// </summary>
    public const string ViewDetail = "permisos.view.detail";
    
    // Permisos de asignación directa a usuarios
    /// <summary>
    /// Permiso para asignar permisos directamente a usuarios
    /// </summary>
    public const string AssignToUsers = "permisos.assign.users";
    
    /// <summary>
    /// Permiso para remover permisos directamente de usuarios
    /// </summary>
    public const string RemoveFromUsers = "permisos.remove.users";
    
    // Administración de permisos
    /// <summary>
    /// Permiso para administrar los permisos del sistema
    /// </summary>
    public const string Admin = "permisos.admin";
}
