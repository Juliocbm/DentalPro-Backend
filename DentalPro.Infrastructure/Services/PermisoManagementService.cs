using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para la gestión básica de permisos (CRUD)
/// </summary>
public class PermisoManagementService : IPermisoManagementService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IPermisoCacheService _cacheService;
    private readonly ILogger<PermisoManagementService> _logger;

    public PermisoManagementService(
        IPermisoRepository permisoRepository,
        IPermisoCacheService cacheService,
        ILogger<PermisoManagementService> logger)
    {
        _permisoRepository = permisoRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los permisos existentes
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetAllPermisosAsync()
    {
        string cacheKey = _cacheService.GetAllPermisosCacheKey();
        return await _cacheService.GetOrCreateCacheAsync(
            cacheKey,
            async () => await _permisoRepository.GetAllAsync(),
            TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    public async Task<Permiso?> GetPermisoByIdAsync(Guid idPermiso)
    {
        return await _permisoRepository.GetByIdAsync(idPermiso);
    }

    /// <summary>
    /// Obtiene un permiso por su nombre
    /// </summary>
    public async Task<Permiso?> GetPermisoByNombreAsync(string nombre)
    {
        return await _permisoRepository.GetByNombreAsync(nombre);
    }

    /// <summary>
    /// Verifica si existe un permiso con el ID especificado
    /// </summary>
    public async Task<bool> ExistsByIdAsync(Guid idPermiso)
    {
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        return permiso != null;
    }
    
    /// <summary>
    /// Verifica si existe un permiso con el nombre especificado
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string nombre)
    {
        var permiso = await _permisoRepository.GetByNombreAsync(nombre);
        return permiso != null;
    }

    /// <summary>
    /// Crea un nuevo permiso
    /// </summary>
    public async Task<Permiso> AddPermisoAsync(Permiso permiso)
    {
        // Verificar si ya existe un permiso con el mismo nombre
        var existePermiso = await _permisoRepository.GetByNombreAsync(permiso.Nombre);
        if (existePermiso != null)
        {
            throw new BadRequestException($"Ya existe un permiso con el nombre '{permiso.Nombre}'");
        }

        await _permisoRepository.AddAsync(permiso);
        await _permisoRepository.SaveChangesAsync();
        
        // Al crear un nuevo permiso solo necesitamos invalidar la caché global de permisos
        await _cacheService.InvalidateAllPermisosCacheAsync();

        return permiso;
    }

    /// <summary>
    /// Actualiza un permiso existente
    /// </summary>
    public async Task<Permiso> UpdatePermisoAsync(Permiso permiso)
    {
        var existePermiso = await _permisoRepository.GetByIdAsync(permiso.IdPermiso);
        if (existePermiso == null)
        {
            throw new NotFoundException("Permiso", permiso.IdPermiso);
        }

        // Verificar si el nuevo nombre ya está en uso por otro permiso
        if (existePermiso.Nombre != permiso.Nombre)
        {
            var permisoConMismoNombre = await _permisoRepository.GetByNombreAsync(permiso.Nombre);
            if (permisoConMismoNombre != null && permisoConMismoNombre.IdPermiso != permiso.IdPermiso)
            {
                throw new BadRequestException($"Ya existe un permiso con el nombre '{permiso.Nombre}'");
            }
        }

        await _permisoRepository.UpdateAsync(permiso);
        await _permisoRepository.SaveChangesAsync();

        // Obtener roles afectados por este permiso para invalidación selectiva de caché
        var rolesAfectados = await _permisoRepository.GetRolesByPermisoIdAsync(permiso.IdPermiso);
        
        // Invalidar caché global de permisos
        await _cacheService.InvalidateAllPermisosCacheAsync();
        
        // Invalidar caché de cada rol afectado y sus usuarios
        foreach (var rol in rolesAfectados)
        {
            await _cacheService.InvalidateRolPermisosCacheAsync(rol.IdRol);
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(rol.IdRol);
        }

        return permiso;
    }

    /// <summary>
    /// Elimina un permiso
    /// </summary>
    public async Task DeletePermisoAsync(Guid idPermiso)
    {
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        // Verificar si es un permiso predeterminado del sistema
        if (permiso.PredeterminadoSistema)
        {
            throw new ForbiddenAccessException("No se pueden eliminar permisos predeterminados del sistema");
        }
        
        // Obtener roles afectados antes de eliminar el permiso
        var rolesAfectados = await _permisoRepository.GetRolesByPermisoIdAsync(idPermiso);

        await _permisoRepository.RemoveAsync(permiso);
        await _permisoRepository.SaveChangesAsync();

        // Invalidar caché global de permisos
        await _cacheService.InvalidateAllPermisosCacheAsync();
        
        // Invalidar caché de roles afectados y sus usuarios
        foreach (var rol in rolesAfectados)
        {
            await _cacheService.InvalidateRolPermisosCacheAsync(rol.IdRol);
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(rol.IdRol);
        }
    }
}
