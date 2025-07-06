using DentalPro.Application.DTOs.Rol;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

public interface IRolService
{
    Task<IEnumerable<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(Guid id);
    Task<Rol?> GetByNombreAsync(string nombre);
    Task<RolDto> CreateAsync(RolDto rol);
    Task<bool> UpdateAsync(RolDto rolDto);
    Task<bool> DeleteAsync(Guid id);
}
