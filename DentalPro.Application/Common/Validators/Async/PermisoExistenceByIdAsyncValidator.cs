using DentalPro.Application.Interfaces.IRepositories;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asíncrono que verifica la existencia de un permiso por su ID
    /// </summary>
    public class PermisoExistenceByIdAsyncValidator<T> : AsyncPropertyValidator<T, Guid>
    {
        private readonly IPermisoRepository _permisoRepository;
        private readonly bool _allowNull;

        /// <summary>
        /// Constructor del validador de existencia de permiso por ID
        /// </summary>
        /// <param name="permisoRepository">Repositorio de permisos</param>
        /// <param name="allowNull">Indica si se permite un valor nulo (Guid.Empty)</param>
        public PermisoExistenceByIdAsyncValidator(IPermisoRepository permisoRepository, bool allowNull = false)
        {
            _permisoRepository = permisoRepository;
            _allowNull = allowNull;
        }

        /// <summary>
        /// Realiza la validación asíncrona
        /// </summary>
        public override string Name => "PermisoExistenceByIdAsync";

        /// <summary>
        /// Valida que exista un permiso con el ID proporcionado
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, Guid permisoId, CancellationToken cancellation)
        {
            // Si el valor es nulo (Guid.Empty) y está permitido, se considera válido
            if (_allowNull && permisoId == Guid.Empty)
                return true;

            // Obtener el permiso del repositorio
            var permiso = await _permisoRepository.GetByIdAsync(permisoId);
            return permiso != null;
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "El permiso con ID '{PropertyValue}' no existe.";
    }

    /// <summary>
    /// Extensiones para facilitar el uso del validador
    /// </summary>
    public static class PermisoExistenceByIdValidatorExtensions
    {
        /// <summary>
        /// Valida que exista un permiso con el ID especificado
        /// </summary>
        public static IRuleBuilderOptions<T, Guid> PermisoExists<T>(this IRuleBuilder<T, Guid> ruleBuilder, IPermisoRepository permisoRepository, bool allowNull = false)
        {
            return ruleBuilder.SetAsyncValidator(new PermisoExistenceByIdAsyncValidator<T>(permisoRepository, allowNull));
        }
    }
}
