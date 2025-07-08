using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthLoginResponseDto>> Login([FromBody] AuthLoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            throw new BadRequestException(ErrorMessages.InvalidCredentials);
        }
        return Ok(result);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<AuthRegisterResponseDto>> Register([FromBody] AuthRegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            throw new BadRequestException("No se pudo registrar el usuario");
        }
        return Ok(result);
    }
}
