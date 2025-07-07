using FluentValidation;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces;
using DentalPro.Application.Common.Validators.Async;

namespace DentalPro.Application.Common.Validators.Usuarios;

/// <summary>
/// Validador para el DTO de actualización de usuario
/// </summary>
public class UpdateUsuarioRequestValidator : AbstractValidator<UpdateUsuarioRequest>
{
    public UpdateUsuarioRequestValidator(IConsultorioService consultorioService, IRolService rolService)
    {
        RuleFor(x => x.IdUsuario)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no debe exceder los 100 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es requerido")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido")
            .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres");

        // Validación para Activo es opcional ya que es un booleano con valor predeterminado

        RuleFor(x => x.Roles)
            .NotNull().WithMessage("La lista de roles no puede ser nula")
            .Must(roles => roles.Count > 0).WithMessage("Debe asignar al menos un rol al usuario")
            .When(x => x.Roles != null);

        RuleForEach(x => x.Roles)
            .NotEmpty().WithMessage("El nombre del rol no puede estar vacío")
            .When(x => x.Roles != null)
            // Validación asincrónica: Verificar que cada rol exista en la base de datos
            .MustExistInDatabase(rolService)
                .WithMessage("El rol '{PropertyValue}' no existe en el sistema");
    }
}
