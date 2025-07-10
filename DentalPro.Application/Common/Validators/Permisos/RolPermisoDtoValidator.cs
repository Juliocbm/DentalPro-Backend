using FluentValidation;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Common.Validators.Async;
using System;
using System.Linq;

namespace DentalPro.Application.Common.Validators.Permisos
{
    public class RolPermisoDtoValidator : AbstractValidator<RolPermisoDto>
    {
        private readonly IRolRepository _rolRepository;
        private readonly IPermisoRepository _permisoRepository;

        public RolPermisoDtoValidator(IRolRepository rolRepository, IPermisoRepository permisoRepository)
        {
            _rolRepository = rolRepository;
            _permisoRepository = permisoRepository;

            RuleFor(x => x.IdRol)
                .NotEmpty().WithMessage("El ID del rol es requerido")
                .RolExists(rolRepository);

            RuleFor(x => x.PermisoIds)
                .NotNull().WithMessage("La lista de permisos no puede ser nula")
                .Must(ids => ids.Any()).WithMessage("Se debe especificar al menos un permiso");

            // Validar que todos los permisos especificados existan
            RuleForEach(x => x.PermisoIds)
                .PermisoExists(permisoRepository);
        }
    }
}
