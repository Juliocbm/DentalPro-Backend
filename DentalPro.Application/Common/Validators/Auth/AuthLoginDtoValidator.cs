using FluentValidation;
using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Common.Validators.Auth
{
    /// <summary>
    /// Validador para las solicitudes de inicio de sesión
    /// </summary>
    public class AuthLoginDtoValidator : AbstractValidator<AuthLoginDto>
    {
        public AuthLoginDtoValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("Por favor, ingrese su dirección de correo electrónico")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido. Ejemplo correcto: usuario@dominio.com")
                .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Por favor, ingrese su contraseña")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }
}
