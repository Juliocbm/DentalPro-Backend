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
    /// Implementación del repositorio para gestionar permisos asignados directamente a usuarios
    /// </summary>
    public class UsuarioPermisoRepository : GenericRepository<UsuarioPermiso>, IUsuarioPermisoRepository
    {
        public UsuarioPermisoRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Verifica si existe una asignación directa de un permiso a un usuario
        /// </summary>
        public async Task<bool> ExistsAsync(Guid idUsuario, Guid idPermiso)
        {
            return await _dbSet.AnyAsync(up => up.IdUsuario == idUsuario && up.IdPermiso == idPermiso);
        }

        /// <summary>
        /// Obtiene todas las asignaciones de permisos para un usuario específico
        /// </summary>
        public async Task<IEnumerable<UsuarioPermiso>> GetByUsuarioIdAsync(Guid idUsuario)
        {
            return await _dbSet
                .Where(up => up.IdUsuario == idUsuario)
                .Include(up => up.Permiso)
                .Include(up => up.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las asignaciones de un permiso específico a distintos usuarios
        /// </summary>
        public async Task<IEnumerable<UsuarioPermiso>> GetByPermisoIdAsync(Guid idPermiso)
        {
            return await _dbSet
                .Where(up => up.IdPermiso == idPermiso)
                .Include(up => up.Permiso)
                .Include(up => up.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Elimina una asignación de permiso a usuario por ID de usuario y permiso
        /// </summary>
        public async Task<bool> DeleteByUsuarioAndPermisoAsync(Guid idUsuario, Guid idPermiso)
        {
            try
            {
                var usuarioPermiso = await _dbSet
                    .FirstOrDefaultAsync(up => up.IdUsuario == idUsuario && up.IdPermiso == idPermiso);

                if (usuarioPermiso == null)
                    return false;

                _dbSet.Remove(usuarioPermiso);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina todas las asignaciones de permisos para un usuario específico
        /// </summary>
        public async Task<bool> DeleteAllByUsuarioAsync(Guid idUsuario)
        {
            try
            {
                var usuarioPermisos = await _dbSet
                    .Where(up => up.IdUsuario == idUsuario)
                    .ToListAsync();

                if (!usuarioPermisos.Any())
                    return true;

                _dbSet.RemoveRange(usuarioPermisos);
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
