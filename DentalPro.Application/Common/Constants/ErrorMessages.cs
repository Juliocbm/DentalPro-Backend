namespace DentalPro.Application.Common.Constants;

/// <summary>
/// Catálogo centralizado de mensajes de error
/// </summary>
public static class ErrorMessages
{
    // Mensajes generales
    public const string DefaultError = "Ha ocurrido un error inesperado.";
    public const string ServiceUnavailable = "El servicio no está disponible en este momento.";
    
    // Mensajes de autenticación
    public const string InvalidCredentials = "Credenciales inválidas.";
    public const string UserNotActive = "El usuario no está activo.";
    public const string UserLocked = "La cuenta de usuario está bloqueada.";
    public const string TokenExpired = "El token ha expirado.";
    public const string InvalidRefreshToken = "El token de refresco es inválido o ha expirado.";
    public const string TokenRevoked = "El token de refresco ha sido revocado.";
    public const string RefreshTokenRequired = "Se requiere un token de refresco válido.";
    
    // Mensajes de autorización
    public const string UnauthorizedAccess = "No está autorizado para realizar esta operación.";
    public const string ForbiddenResource = "No tiene permisos para acceder a este recurso.";
    public const string DifferentConsultorio = "No tiene permisos para acceder a recursos de otro consultorio.";
    
    // Mensajes de recursos
    public const string ResourceNotFound = "El recurso solicitado no existe.";
    public const string ResourceAlreadyExists = "El recurso ya existe.";
    
    // Mensajes de validación
    public const string InvalidDataFormat = "El formato de los datos es inválido.";
    public const string RequiredField = "Este campo es requerido.";
    public const string InvalidEmailFormat = "El formato del correo electrónico es inválido.";
    public const string PasswordMismatch = "Las contraseñas no coinciden.";
    public const string InvalidPasswordFormat = "La contraseña debe tener al menos 8 caracteres, incluir una letra mayúscula, una minúscula y un número.";
    
    // Mensajes específicos para pacientes
    public const string ConsultorioRequired = "Se requiere un consultorio válido.";
    public const string PatientEmailInUse = "El correo electrónico ya está registrado para otro paciente.";
    public const string PatientHasActiveAppointments = "No se puede eliminar el paciente porque tiene citas programadas.";
    public const string PatientHasPendingPayments = "No se puede eliminar el paciente porque tiene pagos pendientes.";
    public const string InvalidDateOfBirth = "La fecha de nacimiento no es válida.";
}
