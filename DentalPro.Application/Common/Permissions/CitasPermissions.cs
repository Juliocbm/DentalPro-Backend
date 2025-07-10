using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para operaciones con citas
/// </summary>
public static class CitasPermissions
{
    public const string View = "citas.view";
    public const string ViewAll = "citas.view.all";
    public const string ViewByDoctor = "citas.view.doctor";
    public const string ViewByPaciente = "citas.view.paciente";
    public const string Create = "citas.create";
    public const string Update = "citas.update";
    public const string Cancel = "citas.cancel";
    public const string Delete = "citas.delete";
}
