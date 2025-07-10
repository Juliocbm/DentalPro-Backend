using System;

namespace DentalPro.Application.Common.Permissions;

/// <summary>
/// Permisos específicos para el módulo de autenticación y seguridad
/// </summary>
public static class AuthPermissions
{
    // Permisos generales
    public const string Login = "auth.login";
    public const string Register = "auth.register";
    public const string RefreshToken = "auth.refresh-token";
    public const string RevokeToken = "auth.revoke-token";
    
    // Permisos para gestión de seguridad (tokens, sesiones, etc.)
    public const string ViewSessions = "auth.sessions.view";
    public const string TerminateSessions = "auth.sessions.terminate";
    
    // Permisos para gestión de dos factores
    public const string ConfigureTwoFactor = "auth.2fa.configure";
    public const string DisableTwoFactor = "auth.2fa.disable";
}
