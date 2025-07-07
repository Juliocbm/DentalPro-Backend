using System.Threading;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces;
using FluentValidation;
using FluentValidation.Validators;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asincrónico para verificar la existencia de roles
    /// </summary>
    /// <typeparam name="T">Tipo del objeto que se está validando</typeparam>
    public class RolExistsValidator<T> : AsyncPropertyValidator<T, string>
    {
        private readonly IRolService _rolService;

        public RolExistsValidator(IRolService rolService)
        {
            _rolService = rolService;
        }

        public override string Name => "RolExistsValidator";
        
        protected override string GetDefaultMessageTemplate(string errorCode)
            => "El rol '{PropertyValue}' no existe en el sistema";

        /// <summary>
        /// Verifica de forma asincrónica si el rol existe en la base de datos
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, string rolNombre, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(rolNombre))
            {
                return false;
            }

            // Verificar en la base de datos si el rol existe
            var exists = await _rolService.ExistsByNameAsync(rolNombre);
            return exists;
        }
    }

    /// <summary>
    /// Extensiones para registrar el validador asincrónico de roles
    /// </summary>
    public static class RolValidatorExtensions
    {
        /// <summary>
        /// Verifica que el rol exista en la base de datos
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustExistInDatabase<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IRolService rolService)
        {
            return ruleBuilder.SetAsyncValidator(new RolExistsValidator<T>(rolService));
        }
    }
}
