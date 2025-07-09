namespace DentalPro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta acceder a un recurso sin permisos
/// </summary>
public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException(string message = "No tiene permisos para acceder a este recurso.") 
        : base(message, "FORBIDDEN_ACCESS")
    {
    }
    
    public ForbiddenAccessException(string message, string errorCode) 
        : base(message, errorCode)
    {
    }
}
