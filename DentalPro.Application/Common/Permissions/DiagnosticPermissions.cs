using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de diagnóstico y pruebas
/// </summary>
public static class DiagnosticPermissions
{
    // Permisos generales
    public const string HealthCheck = "diagnostic.health";
    
    // Permisos para validación
    public const string ValidateDto = "diagnostic.validation";
    
    // Permisos para tokens
    public const string ValidateToken = "diagnostic.auth.token";
    public const string ViewClaims = "diagnostic.auth.claims";
}
