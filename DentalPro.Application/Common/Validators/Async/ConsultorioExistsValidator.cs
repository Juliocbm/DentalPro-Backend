using System;
using System.Threading;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces;
using FluentValidation;
using FluentValidation.Validators;

namespace DentalPro.Application.Common.Validators.Async
{
    /// <summary>
    /// Validador asincrónico para verificar la existencia de un consultorio
    /// </summary>
    /// <typeparam name="T">Tipo del objeto que se está validando</typeparam>
    public class ConsultorioExistsValidator<T> : AsyncPropertyValidator<T, Guid>
    {
        private readonly IConsultorioService _consultorioService;

        public ConsultorioExistsValidator(IConsultorioService consultorioService) 
        {
            _consultorioService = consultorioService;
        }
        
        public override string Name => "ConsultorioExistsValidator";

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "El consultorio con ID '{PropertyValue}' no existe en el sistema";



        /// <summary>
        /// Verifica de forma asincrónica si el consultorio existe en la base de datos
        /// </summary>
        public override async Task<bool> IsValidAsync(ValidationContext<T> context, Guid consultorioId, CancellationToken cancellation)
        {
            if (consultorioId == Guid.Empty)
            {
                return false;
            }

            // Verificar en la base de datos si el consultorio existe
            var exists = await _consultorioService.ExistsByIdAsync(consultorioId);
            return exists;
        }
    }

    /// <summary>
    /// Extensiones para registrar el validador asincrónico de consultorios
    /// </summary>
    public static class ConsultorioValidatorExtensions
    {
        /// <summary>
        /// Verifica que el consultorio exista en la base de datos
        /// </summary>
        public static IRuleBuilderOptions<T, Guid> MustExistInDatabase<T>(
            this IRuleBuilder<T, Guid> ruleBuilder,
            IConsultorioService consultorioService)
        {
            return ruleBuilder.SetAsyncValidator(new ConsultorioExistsValidator<T>(consultorioService));
        }
    }
}
