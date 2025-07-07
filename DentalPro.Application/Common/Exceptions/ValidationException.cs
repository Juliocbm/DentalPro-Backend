using System.Collections.Generic;
using DentalPro.Application.Common.Models;
using FluentValidation.Results;

namespace DentalPro.Application.Common.Exceptions;

/// <summary>
/// Excepción para errores de validación de datos
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException() 
        : base("Se produjeron uno o más errores de validación.", "VALIDATION_FAILED")
    {
        ValidationErrors = new List<ValidationError>();
        FormErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        ValidationErrors = failures
            .Select(f => new ValidationError
            {
                Property = f.PropertyName,
                Message = f.ErrorMessage,
                AttemptedValue = f.AttemptedValue
            })
            .ToList();
            
        // Agrupar errores por propiedad para FormErrors
        FormErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1), // Primera letra en minúscula
                g => g.Select(f => f.ErrorMessage).ToArray()
            );
    }
    
    public ValidationException(string message, string errorCode)
        : base(message, errorCode)
    {
        ValidationErrors = new List<ValidationError>();
        FormErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, string errorCode, Dictionary<string, string[]> formErrors)
        : base(message, errorCode)
    {
        ValidationErrors = new List<ValidationError>();
        FormErrors = formErrors ?? new Dictionary<string, string[]>();
    }

    public IEnumerable<ValidationError> ValidationErrors { get; }
    
    public Dictionary<string, string[]> FormErrors { get; }
}
