using FluentValidation;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Common.Validators.Async;
using DentalPro.Application.Interfaces.IServices;

namespace DentalPro.Application.Common.Validators.Usuarios;

/// <summary>
/// Validador para el DTO de creación de usuario
/// </summary>
public class UsuarioCreateDtoValidator : AbstractValidator<UsuarioCreateDto>
{
    public UsuarioCreateDtoValidator(
        IConsultorioService consultorioService, 
        IRolService rolService, 
        IUsuarioService usuarioService)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no debe exceder los 100 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es requerido")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido")
            .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres")
            .MustAsync(async (correo, cancellation) => 
                !await usuarioService.ExistsByEmailAsync(correo))
                .WithMessage("El correo electrónico ya está en uso");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no debe exceder los 100 caracteres")
            .Must(password => ContainsUppercase(password)).WithMessage("La contraseña debe contener al menos una letra mayúscula")
            .Must(password => ContainsLowercase(password)).WithMessage("La contraseña debe contener al menos una letra minúscula")
            .Must(password => ContainsDigit(password)).WithMessage("La contraseña debe contener al menos un número");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación de la contraseña es requerida")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");

        RuleFor(x => x.IdConsultorio)
            .NotEmpty().WithMessage("El ID del consultorio es requerido")
            // Validación asincrónica: Verificar que el consultorio exista en la base de datos
            .MustExistInDatabase(consultorioService)
                .WithMessage("El consultorio seleccionado no existe en el sistema");

        RuleFor(x => x.RolIds)
            .NotNull().WithMessage("La lista de roles no puede ser nula")
            .Must(roles => roles.Count > 0).WithMessage("Debe asignar al menos un rol al usuario")
            .When(x => x.RolIds != null);

        RuleForEach(x => x.RolIds)
            .NotEqual(Guid.Empty).WithMessage("El ID del rol no puede estar vacío")
            .When(x => x.RolIds != null)
            // Validación asincrónica: Verificar que cada rol exista en la base de datos por ID
            .MustExistInDatabase(rolService)
                .WithMessage("El rol con ID '{PropertyValue}' no existe en el sistema");
    }

    // Métodos auxiliares para validar complejidad de contraseña
    private bool ContainsUppercase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private bool ContainsLowercase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private bool ContainsDigit(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
}
