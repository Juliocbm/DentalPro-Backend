using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = DentalPro.Application.Common.Exceptions.ValidationException;

namespace DentalPro.Application.Common.Validators
{
    /// <summary>
    /// Filtro personalizado para validación que soporta validadores asincrónicos
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Primero validamos si hay errores en el ModelState (validación básica)
            var hasModelStateErrors = !context.ModelState.IsValid;
            
            // También hacemos validación asincrónica manual para los modelos de acción
            var actionArguments = context.ActionArguments;
            var serviceProvider = context.HttpContext.RequestServices;
            var hasAsyncErrors = false;
            var asyncFormErrors = new Dictionary<string, string[]>();
            
            // Validar cada argumento de la acción que tenga un validador registrado
            foreach (var (key, value) in actionArguments)
            {
                if (value == null) continue;
                
                var valueType = value.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(valueType);
                
                // Intentar obtener un validador para este tipo de argumento
                var validator = serviceProvider.GetService(validatorType) as IValidator;
                
                if (validator != null)
                {
                    // Realizar validación asincrónica
                    var validationContext = new ValidationContext<object>(value);
                    var validationResult = await validator.ValidateAsync(validationContext);
                    
                    if (!validationResult.IsValid)
                    {
                        hasAsyncErrors = true;
                        
                        // Agrupar errores por propiedad
                        var errorsByProperty = validationResult.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => ConvertToAngularPropertyName(g.Key),
                                g => g.Select(e => e.ErrorMessage).ToArray());
                        
                        // Agregar a nuestro diccionario de errores
                        foreach (var error in errorsByProperty)
                        {
                            asyncFormErrors[error.Key] = error.Value;
                        }
                    }
                }
            }
            
            // Si hay errores en el ModelState (validación sincrónica)
            if (hasModelStateErrors)
            {
                var errorsInModelState = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage).ToArray());

                var formErrors = new Dictionary<string, string[]>();

                foreach (var error in errorsInModelState)
                {
                    try
                    {
                        // Convertir la clave del modelo de "Usuario.Nombre" a "nombre" para ser compatible con Angular
                        var fieldName = error.Key;
                        
                        if (fieldName.Contains("."))
                        {
                            fieldName = fieldName.Split('.').Last();
                        }
                        
                        // Convertir primera letra a minúscula para compatibilidad con Angular si no está vacía
                        if (!string.IsNullOrEmpty(fieldName) && fieldName.Length > 0)
                        {
                            fieldName = char.ToLowerInvariant(fieldName[0]) + (fieldName.Length > 1 ? fieldName.Substring(1) : "");
                        }
                        
                        formErrors[fieldName] = error.Value;
                    }
                    catch
                    {
                        // En caso de error al procesar el nombre del campo, usar la clave original
                        formErrors[error.Key] = error.Value;
                    }
                }

                // Lanzar nuestra excepción personalizada en lugar de devolver BadRequest directamente
                throw new ValidationException("Datos de entrada inválidos", ErrorCodes.ValidationFailed, formErrors);
            }
            // Si hay errores de validación asincrónica
            else if (hasAsyncErrors)
            {
                throw new ValidationException("Datos de entrada inválidos", ErrorCodes.ValidationFailed, asyncFormErrors);
            }

            // Si el modelo es válido, continuar con la ejecución
            await next();
        }
        
        /// <summary>
        /// Convierte nombres de propiedades a formato compatible con Angular
        /// </summary>
        private string ConvertToAngularPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;
                
            // Si tiene formato de índice como "Roles[0]", lo dejamos así
            if (propertyName.Contains("[") && propertyName.Contains("]"))
                return propertyName.ToLowerInvariant();
                
            // Para propiedades anidadas, tomamos la última parte
            if (propertyName.Contains("."))
                propertyName = propertyName.Split('.').Last();
                
            // Convertir primera letra a minúscula para compatibilidad con Angular
            return char.ToLowerInvariant(propertyName[0]) + 
                   (propertyName.Length > 1 ? propertyName.Substring(1) : "");
        }
    }
}
