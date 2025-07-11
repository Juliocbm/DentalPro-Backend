using FluentValidation;
using DentalPro.Application.DTOs.Consultorio;

namespace DentalPro.Application.DTOs.Consultorio.Validators
{
    /// <summary>
    /// Validador para el DTO de creación de consultorio
    /// </summary>
    public class ConsultorioCreateDtoValidator : AbstractValidator<ConsultorioCreateDto>
    {
        public ConsultorioCreateDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del consultorio es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no debe exceder los 100 caracteres");
                
            RuleFor(x => x.Direccion)
                .MaximumLength(200).WithMessage("La dirección no debe exceder los 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Direccion));
                
            RuleFor(x => x.Telefono)
                .MaximumLength(20).WithMessage("El teléfono no debe exceder los 20 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Telefono));
                
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido")
                .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
