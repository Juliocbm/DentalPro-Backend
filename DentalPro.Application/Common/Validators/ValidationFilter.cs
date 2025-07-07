using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DentalPro.Application.Common.Validators
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Si el modelo no es válido
            if (!context.ModelState.IsValid)
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

            // Si el modelo es válido, continuar con la ejecución
            await next();
        }
    }
}
