using FluentValidation;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Common.Validators.Async;
using DentalPro.Application.Interfaces.IServices;

namespace DentalPro.Application.Common.Validators.Usuarios;

/// <summary>
/// Validador para el DTO de actualización de usuario
/// </summary>
public class UsuarioUpdateDtoValidator : AbstractValidator<UsuarioUpdateDto>
{
    public UsuarioUpdateDtoValidator(
        IRolService rolService, 
        IUsuarioService usuarioService)
    {
        RuleFor(x => x.IdUsuario)
            .NotEmpty().WithMessage("El ID del usuario es requerido")
            .MustAsync(async (id, cancellation) => 
                await usuarioService.ExistsByIdAsync(id))
                .WithMessage("El usuario no existe en el sistema");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no debe exceder los 100 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es requerido")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido")
            .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres")
            .MustAsync(async (model, email, cancellation) => 
                !await usuarioService.ExistsByEmailExceptCurrentAsync(email, model.IdUsuario))
                .WithMessage("El correo electrónico ya está en uso por otro usuario");
                
        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no debe exceder los 20 caracteres")
            .Matches(@"^[0-9+\-\s]*$").WithMessage("El teléfono solo debe contener números, espacios, + o -")
            .When(x => !string.IsNullOrEmpty(x.Telefono));

        RuleFor(x => x.RolIds)
            .NotNull().WithMessage("La lista de roles no puede ser nula")
            .Must(roles => roles.Count > 0).WithMessage("Debe asignar al menos un rol al usuario")
            .When(x => x.RolIds != null);

        RuleForEach(x => x.RolIds)
            .NotEqual(Guid.Empty).WithMessage("El ID del rol no puede estar vacío")
            .When(x => x.RolIds != null)
            // Validación asincrónica: Verificar que cada rol exista en la base de datos por ID
            .SetAsyncValidator(new RolExistenceByIdAsyncValidator<UsuarioUpdateDto>(rolService))
                .WithMessage("El rol con ID '{PropertyValue}' no existe en el sistema");
    }
}
