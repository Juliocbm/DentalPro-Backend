using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de Pacientes
/// </summary>
public static class PacientesPermissions
{
    // Permisos de visualización
    public const string View = "pacientes.view";
    public const string ViewAll = "pacientes.view.all";
    public const string ViewDetail = "pacientes.view.detail";
    
    // Permisos de operaciones
    public const string Create = "pacientes.create";
    public const string Update = "pacientes.update";
    public const string Delete = "pacientes.delete";
    
    // Permisos para historias clínicas
    public const string ViewHistoriaClinica = "pacientes.historia.view";
    public const string UpdateHistoriaClinica = "pacientes.historia.update";
}
