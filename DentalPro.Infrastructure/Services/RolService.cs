using AutoMapper;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DentalPro.Infrastructure.Services;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RolService> _logger;

    public RolService(
        IRolRepository rolRepository, 
        IPermisoRepository permisoRepository,
        IMapper mapper, 
        ICurrentUserService currentUserService,
        ILogger<RolService> logger)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<RolDto>> GetAllAsync()
    {
        // Verificar permiso para ver todos los roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.ViewAll))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver todos los roles sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        var roles = await _rolRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RolDto>>(roles);
    }

    public async Task<RolDto?> GetByIdAsync(Guid id)
    {
        // Verificar permiso para ver detalles de un rol
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.ViewDetail))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver detalles de un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null ? _mapper.Map<RolDto>(rol) : null;
    }

    public async Task<RolDto?> GetByNombreAsync(string nombre)
    {
        // Verificar permiso para ver detalles de un rol
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.ViewDetail))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver detalles de un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        var rol = await _rolRepository.GetByNombreAsync(nombre);
        return rol != null ? _mapper.Map<RolDto>(rol) : null;
    }

    public async Task<RolDto> CreateAsync(RolCreateDto rolCreateDto)
    {
        // Verificar permiso para crear roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.Create))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Las validaciones de duplicados ahora se manejan en el validador RolCreateDtoValidator
        
        // Mapear del DTO de creación a la entidad de dominio
        var rolEntity = _mapper.Map<Rol>(rolCreateDto);
        
        // Generar ID para el nuevo rol
        rolEntity.IdRol = Guid.NewGuid();

        // Guardar la entidad en la base de datos
        await _rolRepository.AddAsync(rolEntity);
        await _rolRepository.SaveChangesAsync();
        
        _logger.LogInformation("Usuario {UserId} creó un nuevo rol con ID {RolId} y nombre {RolNombre}", 
            _currentUserService.GetCurrentUserId(), rolEntity.IdRol, rolEntity.Nombre);
            
        // Retornar el DTO con los datos completos
        return _mapper.Map<RolDto>(rolEntity);
    }

    public async Task<RolDto> UpdateAsync(RolUpdateDto rolUpdateDto)
    {
        // Verificar permiso para actualizar roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.Update))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        var existingRol = await _rolRepository.GetByIdAsync(rolUpdateDto.IdRol);
        if (existingRol == null)
        {
            throw new NotFoundException($"No se encontró el rol con ID {rolUpdateDto.IdRol}", ErrorCodes.ResourceNotFound);
        }

        // Las validaciones de duplicados ahora se manejan en el validador RolUpdateDtoValidator
        
        // Mapear los cambios a la entidad existente
        _mapper.Map(rolUpdateDto, existingRol);

        await _rolRepository.UpdateAsync(existingRol);
        await _rolRepository.SaveChangesAsync();
        
        _logger.LogInformation("Usuario {UserId} actualizó el rol con ID {RolId} y nombre {RolNombre}", 
            _currentUserService.GetCurrentUserId(), existingRol.IdRol, existingRol.Nombre);
            
        // Retornar el DTO con los datos actualizados
        return _mapper.Map<RolDto>(existingRol);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Verificar permiso para eliminar roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.Delete))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null)
        {
            return false;
        }

        await _rolRepository.RemoveAsync(rol);
        await _rolRepository.SaveChangesAsync();
        
        _logger.LogInformation("Usuario {UserId} eliminó el rol con ID {RolId} y nombre {RolNombre}", 
            _currentUserService.GetCurrentUserId(), rol.IdRol, rol.Nombre);
            
        return true;
    }
    
    /// <summary>
    /// Verifica si existe un rol con el nombre especificado
    /// </summary>
    /// <param name="nombre">Nombre del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByNameAsync(string nombre)
    {
        // Este método es usado principalmente por validadores, por lo que no requiere verificación de permiso
        var rol = await _rolRepository.GetByNombreAsync(nombre);
        return rol != null;
    }
    
    /// <summary>
    /// Verifica si existe un rol con el ID especificado
    /// </summary>
    /// <param name="id">ID del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        // Este método es usado principalmente por validadores, por lo que no requiere verificación de permiso
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null;
    }
    
    /// <summary>
    /// Obtiene los permisos asignados a un rol
    /// </summary>
    /// <param name="idRol">ID del rol del que se desean obtener los permisos</param>
    /// <returns>Lista de permisos asociados al rol</returns>
    public async Task<IEnumerable<PermisoDto>> GetPermisosRolAsync(Guid idRol)
    {
        // Verificar permiso para ver permisos de roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.ViewPermisos))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver permisos de un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Verificar si el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            _logger.LogWarning("NotFound: Usuario {UserId} intentó obtener permisos de un rol inexistente con ID {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Obtener permisos del rol
        var permisos = await _permisoRepository.GetByRolIdAsync(idRol);
        
        // Mapear los permisos a DTOs
        return _mapper.Map<IEnumerable<PermisoDto>>(permisos);
    }
    
    /// <summary>
    /// Asigna permisos a un rol por sus IDs
    /// </summary>
    /// <param name="idRol">ID del rol al que se asignarán los permisos</param>
    /// <param name="permisoIds">Lista de IDs de permisos a asignar</param>
    /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
    public async Task<bool> AsignarPermisosRolAsync(Guid idRol, IEnumerable<Guid> permisoIds)
    {
        // Verificar permiso para asignar permisos a roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.AssignPermisos))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar permisos a un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Verificar si el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            _logger.LogWarning("NotFound: Usuario {UserId} intentó asignar permisos a un rol inexistente con ID {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Asignar permisos al rol
        var result = await _permisoRepository.AsignarPermisosARolAsync(idRol, permisoIds);
        
        if (result)
        {
            _logger.LogInformation("Usuario {UserId} asignó permisos al rol {RolId}", 
                _currentUserService.GetCurrentUserId(), idRol);
        }
        else
        {
            _logger.LogWarning("Error: Usuario {UserId} no pudo asignar permisos al rol {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
        }
        
        return result;
    }
    
    /// <summary>
    /// Asigna permisos a un rol por sus nombres
    /// </summary>
    /// <param name="idRol">ID del rol al que se asignarán los permisos</param>
    /// <param name="permisoNombres">Lista de nombres de permisos a asignar</param>
    /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
    public async Task<bool> AsignarPermisosRolByNombreAsync(Guid idRol, IEnumerable<string> permisoNombres)
    {
        // Verificar permiso para asignar permisos a roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.AssignPermisos))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar permisos a un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Verificar si el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            _logger.LogWarning("NotFound: Usuario {UserId} intentó asignar permisos a un rol inexistente con ID {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Obtener los IDs de los permisos por nombre
        var permisoIds = new List<Guid>();
        foreach (var nombre in permisoNombres)
        {
            var permiso = await _permisoRepository.GetByNombreAsync(nombre);
            if (permiso != null)
            {
                permisoIds.Add(permiso.IdPermiso);
            }
            else
            {
                _logger.LogWarning("Permiso con nombre {PermisoNombre} no encontrado al intentar asignar a rol {RolId}",
                    nombre, idRol);
            }
        }
        
        // Si no se encontró ningún permiso, retornar falso
        if (!permisoIds.Any())
        {
            _logger.LogWarning("No se encontró ningún permiso válido para asignar al rol {RolId}", idRol);
            return false;
        }
        
        // Asignar permisos al rol por sus IDs
        return await AsignarPermisosRolAsync(idRol, permisoIds);
    }
    
    /// <summary>
    /// Remueve permisos de un rol por sus IDs
    /// </summary>
    /// <param name="idRol">ID del rol del que se removerán los permisos</param>
    /// <param name="permisoIds">Lista de IDs de permisos a remover</param>
    /// <returns>True si se removieron correctamente, False en caso contrario</returns>
    public async Task<bool> RemoverPermisosRolAsync(Guid idRol, IEnumerable<Guid> permisoIds)
    {
        // Verificar permiso para remover permisos de roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.RemovePermisos))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover permisos de un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Verificar si el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            _logger.LogWarning("NotFound: Usuario {UserId} intentó remover permisos de un rol inexistente con ID {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Remover permisos del rol
        var result = await _permisoRepository.RemoverPermisosDeRolAsync(idRol, permisoIds);
        
        if (result)
        {
            _logger.LogInformation("Usuario {UserId} removió permisos del rol {RolId}", 
                _currentUserService.GetCurrentUserId(), idRol);
        }
        else
        {
            _logger.LogWarning("Error: Usuario {UserId} no pudo remover permisos del rol {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
        }
        
        return result;
    }
    
    /// <summary>
    /// Remueve permisos de un rol por sus nombres
    /// </summary>
    /// <param name="idRol">ID del rol del que se removerán los permisos</param>
    /// <param name="permisoNombres">Lista de nombres de permisos a remover</param>
    /// <returns>True si se removieron correctamente, False en caso contrario</returns>
    public async Task<bool> RemoverPermisosRolByNombreAsync(Guid idRol, IEnumerable<string> permisoNombres)
    {
        // Verificar permiso para remover permisos de roles
        if (!await _currentUserService.HasPermisoAsync(RolesPermissions.RemovePermisos))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover permisos de un rol sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        // Verificar si el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            _logger.LogWarning("NotFound: Usuario {UserId} intentó remover permisos de un rol inexistente con ID {RolId}",
                _currentUserService.GetCurrentUserId(), idRol);
            throw new NotFoundException("Rol", idRol);
        }
        
        // Obtener los IDs de los permisos por nombre
        var permisoIds = new List<Guid>();
        foreach (var nombre in permisoNombres)
        {
            var permiso = await _permisoRepository.GetByNombreAsync(nombre);
            if (permiso != null)
            {
                permisoIds.Add(permiso.IdPermiso);
            }
            else
            {
                _logger.LogWarning("Permiso con nombre {PermisoNombre} no encontrado al intentar remover de rol {RolId}",
                    nombre, idRol);
            }
        }
        
        // Si no se encontró ningún permiso, retornar falso
        if (!permisoIds.Any())
        {
            _logger.LogWarning("No se encontró ningún permiso válido para remover del rol {RolId}", idRol);
            return false;
        }
        
        // Remover permisos del rol por sus IDs
        return await RemoverPermisosRolAsync(idRol, permisoIds);
    }
}
