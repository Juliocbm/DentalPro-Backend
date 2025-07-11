namespace DentalPro.Application.Common.Constants;

/// <summary>
/// Catálogo centralizado de códigos de error
/// </summary>
public static class ErrorCodes
{
    // Errores generales - Rango 1000
    public const string GeneralError = "ERR-1000";
    public const string InvalidOperation = "ERR-1001";
    public const string ConfigurationError = "ERR-1002";
    
    // Errores de autenticación - Rango 2000
    public const string AuthenticationFailed = "ERR-2000";
    public const string InvalidCredentials = "ERR-2001";
    public const string TokenExpired = "ERR-2002";
    public const string InvalidToken = "ERR-2003";
    public const string Unauthorized = "ERR-2004";
    public const string RateLimitExceeded = "ERR-2005";
    public const string TooManyLoginAttempts = "ERR-2006";
    
    // Errores de autorización - Rango 3000
    public const string InsufficientPermissions = "ERR-3000";
    public const string ResourceAccessDenied = "ERR-3001";
    public const string InvalidConsultorio = "ERR-3002";
    public const string ConsultorioNotFound = "ERR-3003";
    public const string UserInactive = "ERR-3004";
    
    // Errores de validación - Rango 4000
    public const string ValidationFailed = "ERR-4000";
    public const string InvalidModelState = "ERR-4001";
    public const string MissingRequiredField = "ERR-4002";
    public const string InvalidId = "ERR-4003";
    
    // Errores de recursos - Rango 5000
    public const string ResourceNotFound = "ERR-5000";
    public const string ResourceAlreadyExists = "ERR-5001";
    public const string ResourceConflict = "ERR-5002";
    public const string DuplicateResourceName = "ERR-5003";
    public const string DuplicateEmail = "ERR-5004";
    
    // Errores de base de datos - Rango 6000
    public const string DatabaseError = "ERR-6000";
    public const string ConcurrencyIssue = "ERR-6001";
    public const string DataIntegrityViolation = "ERR-6002";
    
    // Errores de citas - Rango 7000
    public const string CitaNotFound = "ERR-7000";
    public const string CitaOverlap = "ERR-7001";
    public const string CitaPastDate = "ERR-7002";
    public const string CitaInvalidTimeRange = "ERR-7003";
    public const string CitaCancelled = "ERR-7004";
    public const string CitaOutsideBusinessHours = "ERR-7005";
    public const string PacienteNotFound = "ERR-7006";
    public const string UsuarioNotFound = "ERR-7007";
    public const string UserNotFound = "ERR-7008";
}
