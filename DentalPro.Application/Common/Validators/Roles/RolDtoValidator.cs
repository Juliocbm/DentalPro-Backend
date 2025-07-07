using FluentValidation;
using DentalPro.Application.DTOs.Rol;

namespace DentalPro.Application.Common.Validators.Roles
{
    public class RolDtoValidator : AbstractValidator<RolDto>
    {
        public RolDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es requerido")
                .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres");
                
            // La descripción es opcional, pero si se proporciona debe cumplir con ciertas reglas
            When(x => x.Descripcion != null, () => {
                RuleFor(x => x.Descripcion)
                    .Length(2, 200).WithMessage("La descripción debe tener entre 2 y 200 caracteres");
            });
        }
    }
}
