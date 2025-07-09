using System;
using System.Threading;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces.IServices;
using FluentValidation;
using FluentValidation.Validators;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asincrónico para verificar la existencia de roles por ID
    /// </summary>
    /// <typeparam name="T">Tipo del objeto que se está validando</typeparam>
    public class RolExistenceByIdAsyncValidator<T> : AsyncPropertyValidator<T, Guid>
    {
        private readonly IRolService _rolService;

        public RolExistenceByIdAsyncValidator(IRolService rolService)
        {
            _rolService = rolService;
        }

        public override string Name => "RolExistenceByIdAsyncValidator";
        
        protected override string GetDefaultMessageTemplate(string errorCode)
            => "El rol con ID '{PropertyValue}' no existe en el sistema";

        /// <summary>
        /// Verifica de forma asincrónica si el rol existe en la base de datos por su ID
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, Guid rolId, CancellationToken cancellation)
        {
            if (rolId == Guid.Empty)
            {
                return false;
            }

            // Verificar en la base de datos si el rol existe por ID
            var exists = await _rolService.ExistsByIdAsync(rolId);
            return exists;
        }
    }

    /// <summary>
    /// Extensiones para registrar el validador asincrónico de roles por ID
    /// </summary>
    public static class RolExistenceByIdValidatorExtensions
    {
        /// <summary>
        /// Verifica que el valor exista en la base de datos como un rol válido
        /// </summary>
        public static IRuleBuilderOptions<T, Guid> MustExistInDatabase<T>(
            this IRuleBuilder<T, Guid> ruleBuilder,
            IRolService rolService)
        {
            return ruleBuilder.SetAsyncValidator(new RolExistenceByIdAsyncValidator<T>(rolService));
        }
    }
}
