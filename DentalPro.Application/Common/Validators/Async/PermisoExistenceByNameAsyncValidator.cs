using DentalPro.Application.Interfaces.IRepositories;
using FluentValidation;
using FluentValidation.Validators;
using System.Threading;
using System.Threading.Tasks;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asíncrono que verifica la existencia de un permiso por su nombre
    /// </summary>
    public class PermisoExistenceByNameAsyncValidator<T> : AsyncPropertyValidator<T, string>
    {
        private readonly IPermisoRepository _permisoRepository;
        private readonly bool _mustNotExist;

        /// <summary>
        /// Constructor del validador de existencia de permiso por nombre
        /// </summary>
        /// <param name="permisoRepository">Repositorio de permisos</param>
        /// <param name="mustNotExist">Indica si se está validando que el permiso NO exista (true) o que sí exista (false)</param>
        public PermisoExistenceByNameAsyncValidator(IPermisoRepository permisoRepository, bool mustNotExist)
        {
            _permisoRepository = permisoRepository;
            _mustNotExist = mustNotExist;
        }

        /// <summary>
        /// Realiza la validación asíncrona
        /// </summary>
        public override string Name => "PermisoExistenceByNameAsync";

        /// <summary>
        /// Valida la existencia de un permiso por nombre según el parámetro mustNotExist
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, string nombre, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return !_mustNotExist; // Si es requerido que no exista, devuelve true; de lo contrario, false

            var permiso = await _permisoRepository.GetByNombreAsync(nombre);

            // Si mustNotExist es true, la validación pasa si el permiso NO existe
            // Si mustNotExist es false, la validación pasa si el permiso SÍ existe
            return _mustNotExist ? permiso == null : permiso != null;
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => _mustNotExist ? 
                "Ya existe un permiso con el nombre '{PropertyValue}'." : 
                "No existe ningún permiso con el nombre '{PropertyValue}'.";
    }

    /// <summary>
    /// Extensiones para facilitar el uso del validador
    /// </summary>
    public static class PermisoExistenceByNameValidatorExtensions
    {
        /// <summary>
        /// Valida que NO exista un permiso con el nombre especificado
        /// </summary>
        public static IRuleBuilderOptions<T, string> PermisoNameMustNotExist<T>(this IRuleBuilder<T, string> ruleBuilder, IPermisoRepository permisoRepository)
        {
            return ruleBuilder.SetAsyncValidator(new PermisoExistenceByNameAsyncValidator<T>(permisoRepository, true));
        }

        /// <summary>
        /// Valida que SÍ exista un permiso con el nombre especificado
        /// </summary>
        public static IRuleBuilderOptions<T, string> PermisoNameMustExist<T>(this IRuleBuilder<T, string> ruleBuilder, IPermisoRepository permisoRepository)
        {
            return ruleBuilder.SetAsyncValidator(new PermisoExistenceByNameAsyncValidator<T>(permisoRepository, false));
        }
    }
}
