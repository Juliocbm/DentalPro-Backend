using AutoMapper;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Services;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IMapper _mapper;

    public RolService(IRolRepository rolRepository, IMapper mapper)
    {
        _rolRepository = rolRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Rol>> GetAllAsync()
    {
        return await _rolRepository.GetAllAsync();
    }

    public async Task<Rol?> GetByIdAsync(Guid id)
    {
        return await _rolRepository.GetByIdAsync(id);
    }

    public async Task<Rol?> GetByNombreAsync(string nombre)
    {
        return await _rolRepository.GetByNombreAsync(nombre);
    }

    public async Task<RolDto> CreateAsync(RolDto rolDto)
    {
        // Verificar si ya existe un rol con el mismo nombre
        var existingRol = await _rolRepository.GetByNombreAsync(rolDto.Nombre);
        if (existingRol != null)
        {
            throw new BadRequestException($"Ya existe un rol con el nombre {rolDto.Nombre}", ErrorCodes.DuplicateResourceName);
        }

        // Generar ID si no existe
        if (rolDto.IdRol == Guid.Empty)
        {
            rolDto.IdRol = Guid.NewGuid();
        }

        // Mapear automáticamente del DTO a la entidad de dominio
        var rolEntity = _mapper.Map<Rol>(rolDto);

        // Guardar la entidad en la base de datos
        await _rolRepository.AddAsync(rolEntity);
        await _rolRepository.SaveChangesAsync();
        
        return rolDto;
    }

    public async Task<bool> UpdateAsync(RolDto rolDto)
    {
        var existingRol = await _rolRepository.GetByIdAsync(rolDto.IdRol);
        if (existingRol == null)
        {
            return false;
        }

        // Verificar que no exista otro rol con el mismo nombre
        var rolWithName = await _rolRepository.GetByNombreAsync(rolDto.Nombre);
        if (rolWithName != null && rolWithName.IdRol != rolDto.IdRol)
        {
            throw new BadRequestException($"Ya existe otro rol con el nombre {rolDto.Nombre}", ErrorCodes.DuplicateResourceName);
        }
        
        // Mapear los cambios a la entidad existente
        _mapper.Map(rolDto, existingRol);

        await _rolRepository.UpdateAsync(existingRol);
        await _rolRepository.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null)
        {
            return false;
        }

        await _rolRepository.RemoveAsync(rol);
        await _rolRepository.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Verifica si existe un rol con el nombre especificado
    /// </summary>
    /// <param name="nombre">Nombre del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByNameAsync(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return false;
        }
        
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
        if (id == Guid.Empty)
        {
            return false;
        }
        
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null;
    }
}
