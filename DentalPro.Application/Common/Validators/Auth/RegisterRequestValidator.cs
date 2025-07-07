using FluentValidation;
using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Common.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo electrónico es requerido")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de la contraseña es requerida")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");

            RuleFor(x => x.IdConsultorio)
                .NotEmpty().WithMessage("El ID del consultorio es requerido");
                
            // Los roles son opcionales, pero si se proporcionan, deben ser válidos
            RuleForEach(x => x.Roles)
                .NotEmpty().WithMessage("Los roles no pueden estar vacíos");
        }
    }
}
