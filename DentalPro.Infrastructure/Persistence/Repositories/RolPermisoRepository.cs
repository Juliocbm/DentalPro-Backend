using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio para gestionar relaciones entre roles y permisos
    /// </summary>
    public class RolPermisoRepository : GenericRepository<RolPermiso>, IRolPermisoRepository
    {
        public RolPermisoRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Verifica si existe una relación entre un rol y un permiso específicos
        /// </summary>
        public async Task<bool> ExistsAsync(Guid idRol, Guid idPermiso)
        {
            return await _dbSet.AnyAsync(rp => rp.IdRol == idRol && rp.IdPermiso == idPermiso);
        }

        /// <summary>
        /// Obtiene todas las relaciones rol-permiso para un rol específico
        /// </summary>
        public async Task<IEnumerable<RolPermiso>> GetByRolIdAsync(Guid idRol)
        {
            return await _dbSet
                .Where(rp => rp.IdRol == idRol)
                .Include(rp => rp.Permiso)
                .Include(rp => rp.Rol)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las relaciones rol-permiso para un permiso específico
        /// </summary>
        public async Task<IEnumerable<RolPermiso>> GetByPermisoIdAsync(Guid idPermiso)
        {
            return await _dbSet
                .Where(rp => rp.IdPermiso == idPermiso)
                .Include(rp => rp.Permiso)
                .Include(rp => rp.Rol)
                .ToListAsync();
        }

        /// <summary>
        /// Elimina una relación rol-permiso por ID de rol y permiso
        /// </summary>
        public async Task<bool> DeleteByRolAndPermisoAsync(Guid idRol, Guid idPermiso)
        {
            try
            {
                var rolPermiso = await _dbSet
                    .FirstOrDefaultAsync(rp => rp.IdRol == idRol && rp.IdPermiso == idPermiso);

                if (rolPermiso == null)
                    return false;

                _dbSet.Remove(rolPermiso);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina todas las relaciones rol-permiso para un rol específico
        /// </summary>
        public async Task<bool> DeleteAllByRolAsync(Guid idRol)
        {
            try
            {
                var rolPermisos = await _dbSet
                    .Where(rp => rp.IdRol == idRol)
                    .ToListAsync();

                if (!rolPermisos.Any())
                    return true;

                _dbSet.RemoveRange(rolPermisos);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
