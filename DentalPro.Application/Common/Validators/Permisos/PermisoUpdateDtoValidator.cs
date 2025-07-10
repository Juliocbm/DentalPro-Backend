using FluentValidation;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Common.Validators.Async;
using System;

namespace DentalPro.Application.Common.Validators.Permisos
{
    public class PermisoUpdateDtoValidator : AbstractValidator<PermisoUpdateDto>
    {
        private readonly IPermisoRepository _permisoRepository;

        public PermisoUpdateDtoValidator(IPermisoRepository permisoRepository)
        {
            _permisoRepository = permisoRepository;

            RuleFor(x => x.IdPermiso)
                .NotEmpty().WithMessage("El ID del permiso es requerido")
                .PermisoExists(permisoRepository);

            RuleFor(x => x)
                .MustAsync(async (model, cancellation) => {
                    var existingPermiso = await _permisoRepository.GetByNombreAsync(model.Nombre);
                    return existingPermiso == null || existingPermiso.IdPermiso == model.IdPermiso;
                }).WithMessage(x => $"Ya existe otro permiso con el nombre {x.Nombre}")
                .When(x => !string.IsNullOrEmpty(x.Nombre));

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del permiso es requerido")
                .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres");
                
            // La descripción es opcional, pero si se proporciona debe cumplir con ciertas reglas
            When(x => x.Descripcion != null, () => {
                RuleFor(x => x.Descripcion)
                    .Length(2, 200).WithMessage("La descripción debe tener entre 2 y 200 caracteres");
            });
        }
    }
}
