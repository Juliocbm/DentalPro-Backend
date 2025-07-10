using System;
using System.Threading;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces.IRepositories;
using FluentValidation;
using FluentValidation.Validators;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asincrónico para verificar la existencia de roles por ID usando directamente el repositorio
    /// </summary>
    /// <typeparam name="T">Tipo del objeto que se está validando</typeparam>
    public class RolExistenceByIdRepositoryAsyncValidator<T> : AsyncPropertyValidator<T, Guid>
    {
        private readonly IRolRepository _rolRepository;

        /// <summary>
        /// Constructor del validador
        /// </summary>
        /// <param name="rolRepository">Repositorio de roles</param>
        public RolExistenceByIdRepositoryAsyncValidator(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public override string Name => "RolExistenceByIdRepositoryAsyncValidator";
        
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
            var rol = await _rolRepository.GetByIdAsync(rolId);
            return rol != null;
        }
    }

    /// <summary>
    /// Extensiones para registrar el validador asincrónico de roles por ID usando repositorio
    /// </summary>
    public static class RolExistenceByIdRepositoryValidatorExtensions
    {
        /// <summary>
        /// Verifica que el ID del rol exista en la base de datos
        /// </summary>
        public static IRuleBuilderOptions<T, Guid> RolExists<T>(
            this IRuleBuilder<T, Guid> ruleBuilder,
            IRolRepository rolRepository)
        {
            return ruleBuilder.SetAsyncValidator(new RolExistenceByIdRepositoryAsyncValidator<T>(rolRepository));
        }
    }
}
