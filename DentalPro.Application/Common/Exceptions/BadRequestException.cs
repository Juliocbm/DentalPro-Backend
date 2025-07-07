namespace DentalPro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando la solicitud contiene datos incorrectos
/// </summary>
public class BadRequestException : ApplicationException
{
    public BadRequestException(string message) 
        : base(message, "BAD_REQUEST")
    {
    }
}
