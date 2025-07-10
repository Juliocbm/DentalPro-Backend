using FluentValidation;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Common.Validators.Async;

namespace DentalPro.Application.Common.Validators.Permisos
{
    public class PermisoCreateDtoValidator : AbstractValidator<PermisoCreateDto>
    {
        private readonly IPermisoRepository _permisoRepository;

        public PermisoCreateDtoValidator(IPermisoRepository permisoRepository)
        {
            _permisoRepository = permisoRepository;

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del permiso es requerido")
                .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres")
                .PermisoNameMustNotExist(permisoRepository);
                
            // La descripción es opcional, pero si se proporciona debe cumplir con ciertas reglas
            When(x => x.Descripcion != null, () => {
                RuleFor(x => x.Descripcion)
                    .Length(2, 200).WithMessage("La descripción debe tener entre 2 y 200 caracteres");
            });
        }
    }
}
