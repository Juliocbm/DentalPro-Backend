using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio para la gestión de permisos
/// </summary>
public class PermisoRepository : GenericRepository<Permiso>, IPermisoRepository
{
    public PermisoRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene un permiso por su nombre
    /// </summary>
    public async Task<Permiso> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Nombre.ToLower() == nombre.ToLower());
    }

    /// <summary>
    /// Obtiene los permisos asociados a un rol específico
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByRolIdAsync(Guid rolId)
    {
        return await _context.Set<RolPermiso>()
            .Where(rp => rp.IdRol == rolId)
            .Include(rp => rp.Permiso)
            .Select(rp => rp.Permiso)
            .ToListAsync();
    }

    /// <summary>
    /// Asigna permisos a un rol
    /// </summary>
    public async Task<bool> AsignarPermisosARolAsync(Guid rolId, IEnumerable<Guid> permisoIds)
    {
        try
        {
            // Verificar si el rol existe
            var rol = await _context.Set<Rol>().FindAsync(rolId);
            if (rol == null) return false;

            // Obtener los permisos que ya están asignados al rol para no duplicarlos
            var permisosExistentes = await _context.Set<RolPermiso>()
                .Where(rp => rp.IdRol == rolId)
                .Select(rp => rp.IdPermiso)
                .ToListAsync();

            // Filtrar los permisos que no están ya asignados
            var nuevosPermisos = permisoIds.Where(id => !permisosExistentes.Contains(id)).ToList();

            // Si no hay nuevos permisos para asignar, retornamos true
            if (!nuevosPermisos.Any()) return true;

            // Crear las nuevas asignaciones de rol-permiso
            foreach (var permisoId in nuevosPermisos)
            {
                await _context.Set<RolPermiso>().AddAsync(new RolPermiso
                {
                    IdRol = rolId,
                    IdPermiso = permisoId
                });
            }

            // Guardar cambios
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Remueve permisos de un rol
    /// </summary>
    public async Task<bool> RemoverPermisosDeRolAsync(Guid rolId, IEnumerable<Guid> permisoIds)
    {
        try
        {
            // Verificar si el rol existe
            var rol = await _context.Set<Rol>().FindAsync(rolId);
            if (rol == null) return false;

            // Obtener las relaciones rol-permiso que se deben eliminar
            var relaciones = await _context.Set<RolPermiso>()
                .Where(rp => rp.IdRol == rolId && permisoIds.Contains(rp.IdPermiso))
                .ToListAsync();

            // Si no hay relaciones para eliminar, retornamos true
            if (!relaciones.Any()) return true;

            // Remover las relaciones
            _context.Set<RolPermiso>().RemoveRange(relaciones);

            // Guardar cambios
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Actualiza un permiso existente y devuelve el permiso actualizado
    /// </summary>
    /// <param name="permiso">Permiso con datos actualizados</param>
    /// <returns>Permiso actualizado</returns>
    public new async Task<Permiso> UpdateAsync(Permiso permiso)
    {
        // Llamamos al método base para actualizar la entidad
        await base.UpdateAsync(permiso);

        // Guardamos los cambios
        await _context.SaveChangesAsync();

        // Devolvemos la entidad actualizada
        return permiso;
    }

    /// <summary>
    /// Elimina un permiso por su ID
    /// </summary>
    /// <param name="id">ID del permiso a eliminar</param>
    /// <returns>True si se eliminó correctamente, False si no</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // Buscar el permiso por su ID
            var permiso = await _dbSet.FindAsync(id);
            if (permiso == null) return false;

            // Verificar si el permiso está asignado a algún rol
            bool tieneRoles = await _context.Set<RolPermiso>()
                .AnyAsync(rp => rp.IdPermiso == id);

            // Si tiene roles asignados, primero eliminamos esas relaciones
            if (tieneRoles)
            {
                var relacionesPermiso = await _context.Set<RolPermiso>()
                    .Where(rp => rp.IdPermiso == id)
                    .ToListAsync();

                _context.Set<RolPermiso>().RemoveRange(relacionesPermiso);
            }

            // Eliminar el permiso
            _dbSet.Remove(permiso);

            // Guardar cambios
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
