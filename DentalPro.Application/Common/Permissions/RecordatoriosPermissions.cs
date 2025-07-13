using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de Recordatorios
/// </summary>
public static class RecordatoriosPermissions
{
    #region Permisos de visualización
    
    /// <summary>
    /// Permiso para ver un recordatorio específico
    /// </summary>
    public const string View = "recordatorios.view";
    
    /// <summary>
    /// Permiso para ver todos los recordatorios asociados a una cita
    /// </summary>
    public const string ViewByCita = "recordatorios.view.cita";
    
    /// <summary>
    /// Permiso para ver recordatorios pendientes de enviar
    /// </summary>
    public const string ViewPending = "recordatorios.view.pending";
    
    /// <summary>
    /// Permiso para ver todos los recordatorios (administrativo)
    /// </summary>
    public const string ViewAll = "recordatorios.view.all";
    
    #endregion
    
    #region Permisos de gestión (CRUD)
    
    /// <summary>
    /// Permiso para crear recordatorios
    /// </summary>
    public const string Create = "recordatorios.create";
    
    /// <summary>
    /// Permiso para actualizar recordatorios
    /// </summary>
    public const string Update = "recordatorios.update";
    
    /// <summary>
    /// Permiso para eliminar recordatorios
    /// </summary>
    public const string Delete = "recordatorios.delete";
    
    #endregion
    
    #region Permisos de notificaciones
    
    /// <summary>
    /// Permiso para marcar un recordatorio como enviado
    /// </summary>
    public const string MarkAsSent = "recordatorios.mark-sent";
    
    /// <summary>
    /// Permiso para enviar notificaciones manualmente
    /// </summary>
    public const string SendNotification = "recordatorios.send-notification";
    
    #endregion
