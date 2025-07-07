using FluentValidation;
using DentalPro.Application.DTOs.Usuario;

namespace DentalPro.Application.Common.Validators.Usuarios
{
    public class UsuarioDtoValidator : AbstractValidator<UsuarioDto>
    {
        public UsuarioDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo electrónico es requerido")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido");

            RuleFor(x => x.IdConsultorio)
                .NotEmpty().WithMessage("El ID del consultorio es requerido");
                
            // Los roles son opcionales en la creación, pero si se proporcionan, deben ser válidos
            RuleForEach(x => x.Roles)
                .NotEmpty().WithMessage("Los roles no pueden estar vacíos");
        }
    }
}
