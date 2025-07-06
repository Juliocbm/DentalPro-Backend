using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Este controlador no requiere autenticación
public class TestController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("test")]
    public ActionResult Test()
    {
        // Endpoint simple para verificar que el controlador funciona
        return Ok(new { message = "API funcionando correctamente" });
    }

    [HttpGet("validate-token")]
    public ActionResult ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token no proporcionado");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            
            // Solo para propósitos de diagnóstico, intentamos leer el token sin validarlo
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.Select(c => new {
                Type = c.Type,
                Value = c.Value
            }).ToList();
            
            var roleClaims = claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").ToList();
            
            // Información de validación del token
            return Ok(new
            {
                ValidUntil = jwtToken.ValidTo,
                Issuer = jwtToken.Issuer,
                Audience = jwtToken.Audiences.FirstOrDefault(),
                Claims = claims,
                RoleClaims = roleClaims,
                TokenConfig = new {
                    ConfiguredIssuer = _config["Jwt:Issuer"],
                    ConfiguredAudience = _config["Jwt:Audience"]
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al validar el token", details = ex.Message });
        }
    }
}
