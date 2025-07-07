using AutoMapper;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Application.Interfaces.IServices;

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

    public async Task<IEnumerable<RolDto>> GetAllAsync()
    {
        var roles = await _rolRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RolDto>>(roles);
    }

    public async Task<RolDto?> GetByIdAsync(Guid id)
    {
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null ? _mapper.Map<RolDto>(rol) : null;
    }

    public async Task<RolDto?> GetByNombreAsync(string nombre)
    {
        var rol = await _rolRepository.GetByNombreAsync(nombre);
        return rol != null ? _mapper.Map<RolDto>(rol) : null;
    }

    public async Task<RolDto> CreateAsync(RolCreateDto rolCreateDto)
    {
        // Las validaciones de duplicados ahora se manejan en el validador RolCreateDtoValidator
        
        // Mapear del DTO de creación a la entidad de dominio
        var rolEntity = _mapper.Map<Rol>(rolCreateDto);
        
        // Generar ID para el nuevo rol
        rolEntity.IdRol = Guid.NewGuid();

        // Guardar la entidad en la base de datos
        await _rolRepository.AddAsync(rolEntity);
        await _rolRepository.SaveChangesAsync();
        
        // Retornar el DTO con los datos completos
        return _mapper.Map<RolDto>(rolEntity);
    }

    public async Task<RolDto> UpdateAsync(RolUpdateDto rolUpdateDto)
    {
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
        
        // Retornar el DTO con los datos actualizados
        return _mapper.Map<RolDto>(existingRol);
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
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol != null;
    }
}
