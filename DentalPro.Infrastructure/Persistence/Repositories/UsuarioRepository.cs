using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Correo.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Usuario>> GetByConsultorioAsync(Guid idConsultorio)
    {
        return await _dbSet
            .Where(u => u.IdConsultorio == idConsultorio)
            .Include(u => u.Roles)
                .ThenInclude(r => r.Rol)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid idUsuario)
    {
        var usuario = await _dbSet
            .Include(u => u.Roles)
                .ThenInclude(r => r.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        return usuario?.Roles
            .Select(r => r.Rol?.Nombre ?? string.Empty)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList() ?? new List<string>();
    }

    public async Task<bool> AsignarRolAsync(Guid idUsuario, string rolNombre)
    {
        var usuario = await _dbSet
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return false;

        var rolRepository = new RolRepository(_context);
        var rol = await rolRepository.GetByNombreAsync(rolNombre);

        if (rol == null)
            return false;

        if (usuario.Roles.Any(r => r.IdRol == rol.IdRol))
            return true; // El usuario ya tiene este rol

        usuario.Roles.Add(new UsuarioRol
        {
            IdUsuario = usuario.IdUsuario,
            IdRol = rol.IdRol
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverRolAsync(Guid idUsuario, string rolNombre)
    {
        var usuario = await _dbSet
            .Include(u => u.Roles)
                .ThenInclude(r => r.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return false;

        var rolUsuario = usuario.Roles
            .FirstOrDefault(r => r.Rol != null && r.Rol.Nombre.ToLower() == rolNombre.ToLower());

        if (rolUsuario == null)
            return true; // El usuario no tiene este rol

        usuario.Roles.Remove(rolUsuario);
        await _context.SaveChangesAsync();
        return true;
    }
}
