using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de Usuarios
/// </summary>
public static class UsuariosPermissions
{
    // Permisos de visualización
    public const string View = "usuarios.view";
    public const string ViewAll = "usuarios.view.all";
    public const string ViewDetail = "usuarios.view.detail";
    public const string ViewByConsultorio = "usuarios.view.consultorio";
    public const string ViewAllConsultorios = "usuarios.view.all-consultorios";
    
    // Permisos de operaciones
    public const string Create = "usuarios.create";
    public const string Update = "usuarios.update";
    public const string Delete = "usuarios.delete";
    public const string ChangeStatus = "usuarios.change-status";
    
    // Permisos para gestión de roles
    public const string ViewRoles = "usuarios.roles.view";
    public const string AssignRoles = "usuarios.roles.assign";
    public const string RemoveRoles = "usuarios.roles.remove";
    
    // Permisos para gestión de permisos directos
    public const string ViewPermisos = "usuarios.permisos.view";
    public const string AssignPermisos = "usuarios.permisos.assign";
    public const string RemovePermisos = "usuarios.permisos.remove";
    
    // Permisos para contraseñas
    public const string ResetPassword = "usuarios.password.reset";
    public const string ChangePassword = "usuarios.password.change";
}
