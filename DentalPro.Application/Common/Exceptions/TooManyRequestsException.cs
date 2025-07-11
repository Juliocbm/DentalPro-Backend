using DentalPro.Application.Common.Constants;

namespace DentalPro.Application.Common.Exceptions
{
    /// <summary>
    /// Excepción para indicar que se han realizado demasiadas solicitudes (Rate Limiting)
    /// </summary>
    public class TooManyRequestsException : ApplicationException
    {
        /// <summary>
        /// Constructor para TooManyRequestsException con un mensaje
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public TooManyRequestsException(string message) 
            : base(message, ErrorCodes.RateLimitExceeded)
        {
        }

        /// <summary>
        /// Constructor para TooManyRequestsException con un mensaje y código de error personalizado
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="errorCode">Código de error personalizado</param>
        public TooManyRequestsException(string message, string errorCode)
            : base(message, errorCode)
        {
        }
    }
}
