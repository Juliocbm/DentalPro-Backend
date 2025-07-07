namespace DentalPro.Application.Common.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones de aplicación
/// </summary>
public abstract class ApplicationException : Exception
{
    /// <summary>
    /// Código de error que identifica el tipo específico de error
    /// </summary>
    public string ErrorCode { get; }

    protected ApplicationException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
