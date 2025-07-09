using DentalPro.Application.DTOs.Auth;
using FluentValidation;

namespace DentalPro.Application.Common.Validators.Auth;

public class AuthRefreshTokenDtoValidator : AbstractValidator<AuthRefreshTokenDto>
{
    public AuthRefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El token de refresco es requerido")
            .MaximumLength(256).WithMessage("El token de refresco no puede exceder los 256 caracteres");
    }
}
