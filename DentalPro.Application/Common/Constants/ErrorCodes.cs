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
    
    // Errores de autorización - Rango 3000
    public const string InsufficientPermissions = "ERR-3000";
    public const string ResourceAccessDenied = "ERR-3001";
    public const string InvalidConsultorio = "ERR-3002";
    
    // Errores de validación - Rango 4000
    public const string ValidationFailed = "ERR-4000";
    public const string InvalidModelState = "ERR-4001";
    public const string MissingRequiredField = "ERR-4002";
    
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
}
