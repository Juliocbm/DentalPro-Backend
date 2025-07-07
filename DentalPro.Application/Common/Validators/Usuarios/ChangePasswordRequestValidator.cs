using FluentValidation;
using DentalPro.Application.DTOs.Usuario;

namespace DentalPro.Application.Common.Validators.Usuarios;

/// <summary>
/// Validador para el DTO de cambio de contraseña
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.IdUsuario)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida")
            .MinimumLength(6).WithMessage("La contraseña actual debe tener al menos 6 caracteres");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es requerida")
            .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La nueva contraseña no debe exceder los 100 caracteres")
            .Must(password => ContainsUppercase(password)).WithMessage("La contraseña debe contener al menos una letra mayúscula")
            .Must(password => ContainsLowercase(password)).WithMessage("La contraseña debe contener al menos una letra minúscula")
            .Must(password => ContainsDigit(password)).WithMessage("La contraseña debe contener al menos un número");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("La confirmación de la nueva contraseña es requerida")
            .Equal(x => x.NewPassword).WithMessage("La nueva contraseña y su confirmación no coinciden");

        // Validación personalizada para que la nueva contraseña sea diferente a la actual
        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword).WithMessage("La nueva contraseña debe ser diferente a la actual")
            .OverridePropertyName("NewPassword");
    }

    // Métodos auxiliares para validar complejidad de contraseña
    private bool ContainsUppercase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private bool ContainsLowercase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private bool ContainsDigit(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
}
