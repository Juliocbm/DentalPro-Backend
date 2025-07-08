using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Interfaces.IServices;

public interface IAuthService
{
    Task<AuthLoginResponseDto> LoginAsync(AuthLoginDto request);
    Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterDto request);
}
