using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Validators.Async;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Application.Interfaces.IRepositories;
using FluentValidation;

namespace DentalPro.Application.Common.Validators.Citas;

public class CitaUpdateDtoValidator : AbstractValidator<CitaUpdateDto>
{
    public CitaUpdateDtoValidator(
        PacienteExistenceAsyncValidator pacienteValidator,
        CitaExistenceAsyncValidator citaValidator,
        IDoctorRepository doctorRepository)
    {
        RuleFor(c => c.IdCita)
            .NotEmpty().WithMessage("El identificador de la cita es requerido.")
            .MustAsync(async (id, _) => await citaValidator.ExistsAsync(id))
            .WithMessage(ErrorMessages.CitaNotFound)
            .WithErrorCode(ErrorCodes.CitaNotFound);

        RuleFor(c => c.FechaHoraInicio)
            .NotEmpty().WithMessage("La fecha y hora de inicio son requeridas.")
            .Must(f => f > DateTime.Now).WithMessage(ErrorMessages.CitaPastDate)
            .When(c => c.FechaHoraInicio.Date > DateTime.Now.Date) // Solo validar si la fecha es futura
            .WithErrorCode(ErrorCodes.CitaPastDate);

        RuleFor(c => c.FechaHoraFin)
            .NotEmpty().WithMessage("La fecha y hora de fin son requeridas.")
            .GreaterThan(c => c.FechaHoraInicio)
            .WithMessage(ErrorMessages.CitaInvalidTimeRange)
            .WithErrorCode(ErrorCodes.CitaInvalidTimeRange);

        RuleFor(c => c.Motivo)
            .MaximumLength(250).WithMessage("El motivo no puede exceder los 250 caracteres.");

        RuleFor(c => c.Estatus)
            .NotEmpty().WithMessage("El estatus es requerido.")
            .Must(BeAValidStatus).WithMessage("El estatus no es válido. Valores permitidos: Programada, Confirmada, Cancelada, Completada, Reprogramada.");

        RuleFor(c => c.IdPaciente)
            .NotEmpty().WithMessage("El paciente es requerido.")
            .MustAsync(async (id, _) => await pacienteValidator.ExistsAsync(id))
            .WithMessage(ErrorMessages.PacienteNotFound)
            .WithErrorCode(ErrorCodes.PacienteNotFound);

        RuleFor(c => c.IdDoctor)
            .NotEmpty().WithMessage("El doctor es requerido.")
            .MustAsync(async (id, _) => await doctorRepository.IsUserDoctorAsync(id))
            .WithMessage("El usuario seleccionado no es un doctor")
            .WithErrorCode(ErrorCodes.InvalidOperation);
    }

    private bool BeAValidStatus(string estatus)
    {
        var validStatuses = new[] { "Programada", "Confirmada", "Cancelada", "Completada", "Reprogramada" };
        return validStatuses.Contains(estatus);
    }
}
