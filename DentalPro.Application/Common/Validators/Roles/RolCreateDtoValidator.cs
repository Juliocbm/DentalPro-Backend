using FluentValidation;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces.IRepositories;
using System.Linq;

namespace DentalPro.Application.Common.Validators.Roles
{
    public class RolCreateDtoValidator : AbstractValidator<RolCreateDto>
    {
        private readonly IRolRepository _rolRepository;
        private readonly IPermisoRepository _permisoRepository;

        public RolCreateDtoValidator(IRolRepository rolRepository, IPermisoRepository permisoRepository)
        {
            _rolRepository = rolRepository;
            _permisoRepository = permisoRepository;

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es requerido")
                .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres")
                .MustAsync(async (nombre, cancellation) => {
                    var existingRol = await _rolRepository.GetByNombreAsync(nombre);
                    return existingRol == null;
                }).WithMessage(x => $"Ya existe un rol con el nombre {x.Nombre}");
                
            // La descripción es opcional, pero si se proporciona debe cumplir con ciertas reglas
            When(x => x.Descripcion != null, () => {
                RuleFor(x => x.Descripcion)
                    .Length(2, 200).WithMessage("La descripción debe tener entre 2 y 200 caracteres");
            });
            
            // Los permisos son opcionales, pero si se proporcionan deben existir
            When(x => x.PermisoIds != null && x.PermisoIds.Any(), () => {
                RuleForEach(x => x.PermisoIds)
                    .MustAsync(async (id, cancellation) => {
                        var permiso = await _permisoRepository.GetByIdAsync(id);
                        return permiso != null;
                    }).WithMessage((x, id) => $"El permiso con ID {id} no existe");
            });
        }
    }
}
