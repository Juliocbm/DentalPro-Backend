using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Validators.Async;
using DentalPro.Application.DTOs.Recordatorios;
using FluentValidation;

namespace DentalPro.Application.Common.Validators.Recordatorios;

public class RecordatorioCreateDtoValidator : AbstractValidator<RecordatorioCreateDto>
{
    public RecordatorioCreateDtoValidator(CitaExistenceAsyncValidator citaValidator)
    {
        RuleFor(r => r.IdCita)
            .NotEmpty().WithMessage("El identificador de la cita es requerido.")
            .MustAsync(async (id, _) => await citaValidator.ExistsAsync(id))
            .WithMessage(ErrorMessages.CitaNotFound)
            .WithErrorCode(ErrorCodes.CitaNotFound);

        RuleFor(r => r.Tipo)
            .NotEmpty().WithMessage("El tipo de recordatorio es requerido.")
            .Must(BeAValidType).WithMessage("El tipo de recordatorio no es válido. Valores permitidos: Email, SMS, Notificación.");

        RuleFor(r => r.FechaProgramada)
            .NotEmpty().WithMessage("La fecha programada es requerida.")
            .GreaterThan(DateTime.Now).WithMessage("La fecha programada debe ser futura.");

        RuleFor(r => r.Mensaje)
            .NotEmpty().WithMessage("El mensaje es requerido.")
            .MaximumLength(500).WithMessage("El mensaje no puede exceder los 500 caracteres.");
    }

    private bool BeAValidType(string tipo)
    {
        var validTypes = new[] { "Email", "SMS", "Notificación" };
        return validTypes.Contains(tipo);
    }
}
