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
    }

    public IEnumerable<ValidationError> ValidationErrors { get; }
}
