using DentalPro.Application.Common.Constants;
using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Application.Validators.Common;
using FluentValidation;

namespace DentalPro.Application.DTOs.Recordatorios.Validators;

public class RecordatorioUpdateDtoValidator : AbstractValidator<RecordatorioUpdateDto>
{
    public RecordatorioUpdateDtoValidator(
        CitaExistenceAsyncValidator citaValidator, 
        RecordatorioExistenceAsyncValidator recordatorioValidator)
    {
        RuleFor(r => r.IdRecordatorio)
            .NotEmpty().WithMessage("El identificador del recordatorio es requerido.")
            .MustAsync(async (id, _) => await recordatorioValidator.ExistsAsync(id))
            .WithMessage("El recordatorio no existe.")
            .WithErrorCode(ErrorCodes.ResourceNotFound);

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
            .GreaterThan(DateTime.Now).WithMessage("La fecha programada debe ser futura.")
            .When(r => r.FechaProgramada > DateTime.MinValue); // Solo validar si hay una fecha especificada

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
