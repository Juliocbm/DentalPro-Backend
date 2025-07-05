using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

public interface IRolService
{
    Task<IEnumerable<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(Guid id);
    Task<Rol?> GetByNombreAsync(string nombre);
    Task<Rol> CreateAsync(Rol rol);
    Task<bool> UpdateAsync(Rol rol);
    Task<bool> DeleteAsync(Guid id);
}
