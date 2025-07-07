using FluentValidation;
using System;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Application.Interfaces.IRepositories;

namespace DentalPro.Application.Common.Validators.Pacientes
{
    public class PacienteCreateDtoValidator : AbstractValidator<PacienteCreateDto>
    {
        private readonly IPacienteRepository _pacienteRepository;

        public PacienteCreateDtoValidator(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository;

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Los apellidos son requeridos")
                .MaximumLength(100).WithMessage("Los apellidos no pueden exceder los 100 caracteres");

            RuleFor(x => x.FechaNacimiento)
                .NotEmpty().WithMessage("La fecha de nacimiento es requerida")
                .Must(BeAValidDate).WithMessage("La fecha de nacimiento no es válida")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha de nacimiento no puede ser en el futuro");

            RuleFor(x => x.Telefono)
                .MaximumLength(20).WithMessage("El teléfono no puede exceder los 20 caracteres");

            RuleFor(x => x.Correo)
                .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido")
                .MaximumLength(100).WithMessage("El correo electrónico no puede exceder los 100 caracteres")
                .MustAsync(async (correo, cancellation) => 
                {
                    if (string.IsNullOrEmpty(correo)) return true;
                    var existingPaciente = await _pacienteRepository.GetByCorreoAsync(correo);
                    return existingPaciente == null;
                }).WithMessage("El correo electrónico ya está registrado para otro paciente");
        }

        private bool BeAValidDate(DateTime date)
        {
            return !date.Equals(default(DateTime)) && date.Year > 1900;
        }
    }
}
