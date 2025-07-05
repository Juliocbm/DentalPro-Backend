using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories;

public interface IRolRepository : IGenericRepository<Rol>
{
    Task<Rol?> GetByNombreAsync(string nombre);
}
