using FluentValidation;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces.IRepositories;
using System;

namespace DentalPro.Application.Common.Validators.Roles
{
    public class RolUpdateDtoValidator : AbstractValidator<RolUpdateDto>
    {
        private readonly IRolRepository _rolRepository;

        public RolUpdateDtoValidator(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;

            RuleFor(x => x.IdRol)
                .NotEmpty().WithMessage("El ID del rol es requerido");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es requerido")
                .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres")
                .MustAsync(async (model, nombre, cancellation) => {
                    var existingRol = await _rolRepository.GetByNombreAsync(nombre);
                    return existingRol == null || existingRol.IdRol == model.IdRol;
                }).WithMessage(x => $"Ya existe otro rol con el nombre {x.Nombre}");
                
            // La descripción es opcional, pero si se proporciona debe cumplir con ciertas reglas
            When(x => x.Descripcion != null, () => {
                RuleFor(x => x.Descripcion)
                    .Length(2, 200).WithMessage("La descripción debe tener entre 2 y 200 caracteres");
            });
        }
    }
}
