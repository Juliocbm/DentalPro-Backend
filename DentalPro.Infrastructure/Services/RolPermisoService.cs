using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para la gestión de permisos asignados a roles
/// </summary>
public class RolPermisoService : IRolPermisoService
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolPermisoRepository _rolPermisoRepository;
    private readonly IPermisoCacheService _permisoCacheService;
    private readonly ILogger<RolPermisoService> _logger;
    
    public RolPermisoService(
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository,
        IRolPermisoRepository rolPermisoRepository,
        IPermisoCacheService permisoCacheService,
        ILogger<RolPermisoService> logger)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
        _rolPermisoRepository = rolPermisoRepository;
        _permisoCacheService = permisoCacheService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol)
    {
        _logger.LogInformation("Obteniendo permisos para el rol con ID: {IdRol}", idRol);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Intentamos obtener los permisos desde caché primero
        var cacheKey = $"rol_permisos_{idRol}";
        return await _permisoCacheService.GetOrCreateCacheAsync(
            cacheKey,
            async () => await _rolPermisoRepository.GetPermisosByRolIdAsync(idRol),
            TimeSpan.FromMinutes(10)
        );
    }
    
    public async Task<IEnumerable<Permiso>> GetPermisosByRolNameAsync(string nombreRol)
    {
        _logger.LogInformation("Obteniendo permisos para el rol con nombre: {NombreRol}", nombreRol);
        
        // Obtenemos el rol por su nombre
        var rol = await _rolRepository.GetByNombreAsync(nombreRol);
        if (rol == null)
        {
            _logger.LogWarning("El rol con nombre {NombreRol} no existe", nombreRol);
            throw new NotFoundException("Rol", nombreRol);
        }
        
        // Delegamos a GetPermisosByRolIdAsync para aprovechar la caché
        return await GetPermisosByRolIdAsync(rol.IdRol);
    }
    
    public async Task<bool> HasRolPermisoByNameAsync(Guid idRol, string nombrePermiso)
    {
        _logger.LogInformation("Verificando si el rol {IdRol} tiene el permiso {NombrePermiso}", idRol, nombrePermiso);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos si el permiso existe
        var permiso = await _permisoRepository.GetByNombreAsync(nombrePermiso);
        if (permiso == null)
        {
            _logger.LogWarning("El permiso con nombre {NombrePermiso} no existe", nombrePermiso);
            return false;
        }
        
        return await _rolPermisoRepository.ExistsAsync(idRol, permiso.IdPermiso);
    }
    
    public async Task<bool> HasRolPermisoByIdAsync(Guid idRol, Guid idPermiso)
    {
        _logger.LogInformation("Verificando si el rol {IdRol} tiene el permiso {IdPermiso}", idRol, idPermiso);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos que el permiso exista
        var permisoExiste = await _permisoRepository.ExistsAsync(idPermiso);
        if (!permisoExiste)
        {
            _logger.LogWarning("El permiso con ID {IdPermiso} no existe", idPermiso);
            return false;
        }
        
        return await _rolPermisoRepository.ExistsAsync(idRol, idPermiso);
    }
    
    public async Task<bool> AsignarPermisoAsync(Guid idRol, Guid idPermiso)
    {
        _logger.LogInformation("Asignando permiso {IdPermiso} al rol {IdRol}", idPermiso, idRol);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos que el permiso exista
        var permisoExiste = await _permisoRepository.ExistsAsync(idPermiso);
        if (!permisoExiste)
        {
            _logger.LogWarning("El permiso con ID {IdPermiso} no existe", idPermiso);
            throw new NotFoundException("Permiso", idPermiso);
        }
        
        // Asignamos el permiso
        var resultado = await _rolPermisoRepository.AsignarPermisoAsync(idRol, idPermiso);
        
        // Invalidamos la caché si se asignó correctamente
        if (resultado)
        {
            await _permisoCacheService.InvalidateRolPermisosCacheAsync(idRol);
            await _permisoCacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }
        
        return resultado;
    }
    
    public async Task<bool> AsignarPermisosAsync(Guid idRol, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Asignando múltiples permisos al rol {IdRol}", idRol);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos que todos los permisos existan
        var permisosArray = idsPermisos.ToArray();
        foreach (var idPermiso in permisosArray)
        {
            var permisoExiste = await _permisoRepository.ExistsAsync(idPermiso);
            if (!permisoExiste)
            {
                _logger.LogWarning("El permiso con ID {IdPermiso} no existe", idPermiso);
                throw new NotFoundException("Permiso", idPermiso);
            }
        }
        
        // Asignamos los permisos
        var resultado = true;
        foreach (var idPermiso in permisosArray)
        {
            var asignado = await _rolPermisoRepository.AsignarPermisoAsync(idRol, idPermiso);
            resultado = resultado && asignado;
        }
        
        // Invalidamos la caché si se asignó al menos un permiso
        if (resultado)
        {
            await _permisoCacheService.InvalidateRolPermisosCacheAsync(idRol);
            await _permisoCacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }
        
        return resultado;
    }
    
    public async Task<bool> RemoverPermisoAsync(Guid idRol, Guid idPermiso)
    {
        _logger.LogInformation("Removiendo permiso {IdPermiso} del rol {IdRol}", idPermiso, idRol);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos que el permiso exista
        var permisoExiste = await _permisoRepository.ExistsAsync(idPermiso);
        if (!permisoExiste)
        {
            _logger.LogWarning("El permiso con ID {IdPermiso} no existe", idPermiso);
            throw new NotFoundException("Permiso", idPermiso);
        }
        
        // Removemos el permiso
        var resultado = await _rolPermisoRepository.RemoverPermisoAsync(idRol, idPermiso);
        
        // Invalidamos la caché si se removió correctamente
        if (resultado)
        {
            await _permisoCacheService.InvalidateRolPermisosCacheAsync(idRol);
            await _permisoCacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }
        
        return resultado;
    }
    
    public async Task<bool> RemoverPermisosAsync(Guid idRol, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Removiendo múltiples permisos del rol {IdRol}", idRol);
        
        // Verificamos que el rol exista
        var rolExiste = await _rolRepository.ExistsAsync(idRol);
        if (!rolExiste)
        {
            _logger.LogWarning("El rol con ID {IdRol} no existe", idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Verificamos que todos los permisos existan
        var permisosArray = idsPermisos.ToArray();
        foreach (var idPermiso in permisosArray)
        {
            var permisoExiste = await _permisoRepository.ExistsAsync(idPermiso);
            if (!permisoExiste)
            {
                _logger.LogWarning("El permiso con ID {IdPermiso} no existe", idPermiso);
                throw new NotFoundException("Permiso", idPermiso);
            }
        }
        
        // Removemos los permisos
        var resultado = true;
        foreach (var idPermiso in permisosArray)
        {
            var removido = await _rolPermisoRepository.RemoverPermisoAsync(idRol, idPermiso);
            resultado = resultado && removido;
        }
        
        // Invalidamos la caché si se removió al menos un permiso
        if (resultado)
        {
            await _permisoCacheService.InvalidateRolPermisosCacheAsync(idRol);
            await _permisoCacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }
        
        return resultado;
    }
}
