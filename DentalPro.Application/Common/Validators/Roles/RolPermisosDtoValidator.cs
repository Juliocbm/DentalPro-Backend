using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces.IServices;
using FluentValidation;

namespace DentalPro.Application.Common.Validators.Roles;

/// <summary>
/// Validador para el DTO de asignación/remoción de permisos a roles
/// </summary>
public class RolPermisosDtoValidator : AbstractValidator<RolPermisosDto>
{
    private readonly IRolService _rolService;
    private readonly IPermisoManagementService _permisoManagementService;
    
    public RolPermisosDtoValidator(
        IRolService rolService,
        IPermisoManagementService permisoManagementService)
    {
        _rolService = rolService;
        _permisoManagementService = permisoManagementService;
        
        RuleFor(x => x.IdRol)
            .NotEmpty().WithMessage("El ID del rol es obligatorio")
            .MustAsync(async (id, cancellation) => 
            {
                return await _rolService.ExistsByIdAsync(id);
            }).WithMessage("El rol especificado no existe");
        
        // Validar que los IDs de permisos existan si se proporcionan
        When(x => x.PermisoIds != null && x.PermisoIds.Count > 0, () => 
        {
            RuleForEach(x => x.PermisoIds)
                .MustAsync(async (id, cancellation) => 
                {
                    return await _permisoManagementService.ExistsByIdAsync(id);
                }).WithMessage("Uno o más permisos especificados no existen");
        });
        
        // Validar que los nombres de permisos existan si se proporcionan
        When(x => x.PermisoNombres != null && x.PermisoNombres.Count > 0, () => 
        {
            RuleFor(x => x.PermisoNombres)
                .MustAsync(async (nombres, cancellation) => 
                {
                    foreach (var nombre in nombres)
                    {
                        if (!await _permisoManagementService.ExistsByNameAsync(nombre))
                        {
                            return false;
                        }
                    }
                    return true;
                }).WithMessage("Uno o más permisos especificados por nombre no existen");
        });
        
        // Al menos uno de los dos campos debe tener valores
        RuleFor(x => x)
            .Must(dto => 
                dto.PermisoIds != null && dto.PermisoIds.Count > 0 || 
                dto.PermisoNombres != null && dto.PermisoNombres.Count > 0)
            .WithMessage("Debe proporcionar al menos un permiso (ya sea por ID o por nombre)");
    }
}
