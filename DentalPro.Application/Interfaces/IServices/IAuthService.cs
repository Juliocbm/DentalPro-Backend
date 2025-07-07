using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Interfaces.IServices;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
