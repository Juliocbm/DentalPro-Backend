using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using FluentValidation;

namespace DentalPro.Application.Validators.Usuario;

public class UsuarioPermisosDtoValidator : AbstractValidator<UsuarioPermisosDto>
{
    private readonly IUsuarioService _usuarioService;
    private readonly IPermisoService _permisoService;
    
    public UsuarioPermisosDtoValidator(
        IUsuarioService usuarioService,
        IPermisoService permisoService)
    {
        _usuarioService = usuarioService;
        _permisoService = permisoService;
        
        RuleFor(x => x.IdUsuario)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .MustAsync(async (id, cancellation) => 
            {
                return await _usuarioService.ExistsByIdAsync(id);
            }).WithMessage("El usuario especificado no existe");
        
        // Si se proporcionan IDs de permisos, validarlos
        When(x => x.PermisoIds != null && x.PermisoIds.Count > 0, () => 
        {
            RuleForEach(x => x.PermisoIds)
                .MustAsync(async (id, cancellation) => 
                {
                    var permiso = await _permisoService.GetPermisoByIdAsync(id);
                    return permiso != null;
                }).WithMessage("Uno o más permisos especificados no existen");
        });
        
        // Si se proporcionan nombres de permisos, validarlos
        When(x => x.PermisoNombres != null && x.PermisoNombres.Count > 0, () => 
        {
            RuleForEach(x => x.PermisoNombres)
                .NotEmpty().WithMessage("El nombre del permiso no puede estar vacío")
                .MustAsync(async (nombre, cancellation) => 
                {
                    return await _permisoService.ExistsPermisoByNombreAsync(nombre);
                }).WithMessage("Uno o más permisos especificados por nombre no existen");
        });
        
        // Al menos uno de los dos campos debe tener valores
        RuleFor(x => x)
            .Must(dto => 
                (dto.PermisoIds != null && dto.PermisoIds.Count > 0) || 
                (dto.PermisoNombres != null && dto.PermisoNombres.Count > 0))
            .WithMessage("Debe proporcionar al menos un permiso (ya sea por ID o por nombre)");
    }
}
