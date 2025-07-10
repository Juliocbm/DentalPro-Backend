using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de Roles
/// </summary>
public static class RolesPermissions
{
    // Permisos de visualización
    public const string View = "roles.view";
    public const string ViewAll = "roles.view.all";
    public const string ViewDetail = "roles.view.detail";
    
    // Permisos de operaciones
    public const string Create = "roles.create";
    public const string Update = "roles.update";
    public const string Delete = "roles.delete";
    
    // Permisos para gestión de permisos en roles
    public const string AssignPermisos = "roles.permisos.assign";
    public const string RemovePermisos = "roles.permisos.remove";
    public const string ViewPermisos = "roles.permisos.view";
}
