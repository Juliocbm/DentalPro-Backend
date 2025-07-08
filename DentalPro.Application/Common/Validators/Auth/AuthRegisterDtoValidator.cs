using FluentValidation;
using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Common.Validators.Auth
{
    /// <summary>
    /// Validador para las solicitudes de registro de usuarios
    /// </summary>
    public class AuthRegisterDtoValidator : AbstractValidator<AuthRegisterDto>
    {
        public AuthRegisterDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("Por favor, ingrese su nombre completo")
                .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres")
                .Matches("^[\\p{L}\\s\\.'-]+$").WithMessage("El nombre solo debe contener letras, espacios y caracteres como .' -");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("Por favor, ingrese su dirección de correo electrónico")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido. Ejemplo correcto: usuario@dominio.com")
                .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Por favor, ingrese una contraseña")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no debe exceder los 100 caracteres")
                .Must(password => ContainsUppercase(password)).WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Must(password => ContainsLowercase(password)).WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Must(password => ContainsDigit(password)).WithMessage("La contraseña debe contener al menos un número");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Por favor, confirme su contraseña")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden. Por favor verifique");

            RuleFor(x => x.IdConsultorio)
                .NotEmpty().WithMessage("Por favor, seleccione un consultorio");
                
            RuleFor(x => x.Roles)
                .NotNull().WithMessage("La lista de roles no puede ser nula")
                .Must(roles => roles.Count > 0).WithMessage("Debe asignar al menos un rol al usuario");
        }

        private bool ContainsUppercase(string password)
        {
            return password.Any(char.IsUpper);
        }

        private bool ContainsLowercase(string password)
        {
            return password.Any(char.IsLower);
        }

        private bool ContainsDigit(string password)
        {
            return password.Any(char.IsDigit);
        }
    }
}
