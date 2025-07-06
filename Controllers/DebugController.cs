using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Solo requiere autenticación, no un rol específico
public class DebugController : ControllerBase
{
    [HttpGet("claims")]
    public ActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new 
        {
            Type = c.Type,
            Value = c.Value
        }).ToList();
        
        // Verificar específicamente los claims de rol
        var rolesClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        // Verificar si el usuario está en el rol de Administrador
        var isAdmin = User.IsInRole("Administrador");
        
        return Ok(new 
        { 
            AllClaims = claims,
            Roles = rolesClaims,
            IsInAdminRole = isAdmin
        });
    }
}
