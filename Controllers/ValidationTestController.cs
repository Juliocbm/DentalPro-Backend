using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationTestController : ControllerBase
{
    private readonly IValidator<LoginRequest> _loginValidator;

    public ValidationTestController(IValidator<LoginRequest> loginValidator)
    {
        _loginValidator = loginValidator;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateLoginRequest([FromBody] LoginRequest request)
    {
        // Validar manualmente usando FluentValidation
        var validationResult = await _loginValidator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            // Crear un diccionario de errores para Angular FormGroup
            var formErrors = new Dictionary<string, string[]>();
            
            foreach (var error in validationResult.Errors.GroupBy(e => e.PropertyName))
            {
                var fieldName = error.Key;
                
                // Convertir primera letra a minúscula para compatibilidad con Angular
                if (!string.IsNullOrEmpty(fieldName) && fieldName.Length > 0)
                {
                    fieldName = char.ToLowerInvariant(fieldName[0]) + (fieldName.Length > 1 ? fieldName.Substring(1) : "");
                }
                
                formErrors[fieldName] = error.Select(e => e.ErrorMessage).ToArray();
            }
            
            // Lanzar nuestra excepción personalizada
            throw new Application.Common.Exceptions.ValidationException("Datos de entrada inválidos", ErrorCodes.ValidationFailed, formErrors);
        }
        
        return Ok(new { message = "Validación exitosa" });
    }
}
