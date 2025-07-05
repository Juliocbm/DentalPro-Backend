using DentalPro.Application.Interfaces;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Services;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;

    public RolService(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
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

    public async Task<Rol> CreateAsync(Rol rol)
    {
        // Verificar si ya existe un rol con el mismo nombre
        var existingRol = await _rolRepository.GetByNombreAsync(rol.Nombre);
        if (existingRol != null)
        {
            throw new Exception($"Ya existe un rol con el nombre {rol.Nombre}");
        }

        // Generar ID si no existe
        if (rol.IdRol == Guid.Empty)
        {
            rol.IdRol = Guid.NewGuid();
        }

        var result = await _rolRepository.AddAsync(rol);
        await _rolRepository.SaveChangesAsync();
        
        return result;
    }

    public async Task<bool> UpdateAsync(Rol rol)
    {
        var existingRol = await _rolRepository.GetByIdAsync(rol.IdRol);
        if (existingRol == null)
        {
            return false;
        }

        // Verificar que no exista otro rol con el mismo nombre
        var rolWithName = await _rolRepository.GetByNombreAsync(rol.Nombre);
        if (rolWithName != null && rolWithName.IdRol != rol.IdRol)
        {
            throw new Exception($"Ya existe otro rol con el nombre {rol.Nombre}");
        }

        await _rolRepository.UpdateAsync(rol);
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
}
