using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Rol?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombre.ToLower());
    }
}
