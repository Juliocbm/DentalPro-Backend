using System.Text.Json.Serialization;

namespace DentalPro.Application.Common.Models;

/// <summary>
/// Modelo de respuesta de error estandarizado para la API, optimizado para compatibilidad con Angular
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código de estado HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Código interno de error para identificación
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Campo de error principal para compatibilidad con Angular
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensaje amigable para el usuario
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalles técnicos del error (solo en desarrollo)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Errores de validación si los hay
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<ValidationError>? ValidationErrors { get; set; }

    /// <summary>
    /// Errores de formulario en formato compatible con Angular FormGroup
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? FormErrors { get; set; }

    /// <summary>
    /// Marca de tiempo cuando ocurrió el error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para errores de validación
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Propiedad que causó el error de validación
    /// </summary>
    public string Property { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensaje de error
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor proporcionado
    /// </summary>
    public object? AttemptedValue { get; set; }
}
