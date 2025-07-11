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
        /// Agrega múltiples relaciones rol-permiso en una sola operación
        /// </summary>
        /// <param name="rolPermisos">Lista de relaciones rol-permiso a agregar</param>
        /// <returns>True si se agregaron correctamente, False si no</returns>
        public async Task<bool> AddRangeAsync(IEnumerable<RolPermiso> rolPermisos)
        {
            try
            {
                // Verificar que no existan las relaciones antes de agregarlas
                var idsToAdd = new List<(Guid idRol, Guid idPermiso)>();
                foreach (var rolPermiso in rolPermisos)
                {
                    idsToAdd.Add((rolPermiso.IdRol, rolPermiso.IdPermiso));
                }

                // Filtrar las relaciones que ya existen
                var existingRelations = await _dbSet
                    .Where(rp => idsToAdd.Any(id => id.idRol == rp.IdRol && id.idPermiso == rp.IdPermiso))
                    .ToListAsync();

                // Obtener solo las relaciones que no existen ya
                var newRelations = rolPermisos.Where(rp =>
                    !existingRelations.Any(er => er.IdRol == rp.IdRol && er.IdPermiso == rp.IdPermiso));

                if (newRelations.Any())
                {
                    await _dbSet.AddRangeAsync(newRelations);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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


        /// <summary>
        /// Obtiene los permisos asociados a un rol específico
        /// </summary>
        public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol)
        {
            var rolPermisos = await GetByRolIdAsync(idRol);
            return rolPermisos.Select(rp => rp.Permiso).ToList();
        }

        /// <summary>
        /// Asigna un permiso a un rol
        /// </summary>
        public async Task<bool> AsignarPermisoAsync(Guid idRol, Guid idPermiso)
        {
            try
            {
                // Verificar si la relación ya existe
                var existe = await ExistsAsync(idRol, idPermiso);
                if (existe)
                    return true; // Ya existe, consideramos éxito

                // Crear nueva relación
                var rolPermiso = new RolPermiso
                {
                    IdRol = idRol,
                    IdPermiso = idPermiso
                };

                await _dbSet.AddAsync(rolPermiso);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Remueve un permiso de un rol
        /// </summary>
        public async Task<bool> RemoverPermisoAsync(Guid idRol, Guid idPermiso)
        {
            // Reutilizamos el método DeleteByRolAndPermisoAsync ya existente
            return await DeleteByRolAndPermisoAsync(idRol, idPermiso);
        }
    }
}
