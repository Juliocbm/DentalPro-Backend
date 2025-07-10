using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de Recordatorios
/// </summary>
public static class RecordatoriosPermissions
{
    // Permisos de visualización
    public const string View = "recordatorios.view";
    public const string ViewByCita = "recordatorios.view.cita";
    public const string ViewPending = "recordatorios.view.pending";
    
    // Permisos de operaciones
    public const string Create = "recordatorios.create";
    public const string Update = "recordatorios.update";
    public const string MarkAsSent = "recordatorios.mark-sent";
    public const string Delete = "recordatorios.delete";
}
