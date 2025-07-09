using System.Threading;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces.IServices;
using FluentValidation;
using FluentValidation.Validators;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asincrónico para verificar la existencia de roles por nombre
    /// </summary>
    /// <typeparam name="T">Tipo del objeto que se está validando</typeparam>
    public class RolExistenceByNameAsyncValidator<T> : AsyncPropertyValidator<T, string>
    {
        private readonly IRolService _rolService;

        public RolExistenceByNameAsyncValidator(IRolService rolService)
        {
            _rolService = rolService;
        }

        public override string Name => "RolExistenceByNameAsyncValidator";
        
        protected override string GetDefaultMessageTemplate(string errorCode)
            => "El rol '{PropertyValue}' no existe en el sistema";

        /// <summary>
        /// Verifica de forma asincrónica si el rol existe en la base de datos por su nombre
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, string rolNombre, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(rolNombre))
            {
                return false;
            }

            // Verificar en la base de datos si el rol existe por nombre
            var exists = await _rolService.ExistsByNameAsync(rolNombre);
            return exists;
        }
    }

    /// <summary>
    /// Extensiones para registrar el validador asincrónico de roles por nombre
    /// </summary>
    public static class RolExistenceByNameValidatorExtensions
    {
        /// <summary>
        /// Verifica que el rol exista en la base de datos por su nombre
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustExistInDatabase<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IRolService rolService)
        {
            return ruleBuilder.SetAsyncValidator(new RolExistenceByNameAsyncValidator<T>(rolService));
        }
    }
}
