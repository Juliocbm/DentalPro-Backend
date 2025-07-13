namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Define los permisos específicos para el módulo de consultorios
/// </summary>
public static class ConsultoriosPermissions
{
    #region Consultorios CRUD
    
    /// <summary>
    /// Permiso para ver detalles de un consultorio
    /// </summary>
    public const string ViewDetail = "consultorios.view";
    
    /// <summary>
    /// Permiso para ver todos los consultorios
    /// </summary>
    public const string ViewAll = "consultorios.view.all";
    
    /// <summary>
    /// Permiso para crear consultorios
    /// </summary>
    public const string Create = "consultorios.create";
    
    /// <summary>
    /// Permiso para actualizar consultorios
    /// </summary>
    public const string Update = "consultorios.update";
    
    /// <summary>
    /// Permiso para eliminar consultorios
    /// </summary>
    public const string Delete = "consultorios.delete";
    
    #endregion
    
    #region Gestión de personal
    
    /// <summary>
    /// Permiso para ver doctores de un consultorio
    /// </summary>
    public const string ViewDoctores = "consultorios.view.doctores";
    
    /// <summary>
    /// Permiso para ver asistentes de un consultorio
    /// </summary>
    public const string ViewAsistentes = "consultorios.view.asistentes";
    
    /// <summary>
    /// Permiso para asignar personal a un consultorio
    /// </summary>
    public const string AssignStaff = "consultorios.assign.staff";
    
    /// <summary>
    /// Permiso para desvincular personal de un consultorio
    /// </summary>
    public const string RemoveStaff = "consultorios.remove.staff";
    
    #endregion
}
