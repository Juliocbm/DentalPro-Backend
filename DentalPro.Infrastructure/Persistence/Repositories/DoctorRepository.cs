using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica si un usuario tiene el rol de Doctor
        /// </summary>
        public async Task<bool> IsUserDoctorAsync(Guid userId)
        {
            return await _context.UsuarioRoles
                .Where(ur => ur.IdUsuario == userId)
                .Join(_context.Roles,
                    ur => ur.IdRol,
                    r => r.IdRol,
                    (ur, r) => r.Nombre.ToUpper())
                .AnyAsync(name => name == "DOCTOR");
        }
        
        /// <summary>
        /// Obtiene todos los usuarios con rol de Doctor en un consultorio específico
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetAllDoctorsAsync(Guid consultorioId)
        {
            return await _context.Usuarios
                .Where(u => u.IdConsultorio == consultorioId)
                .Join(_context.UsuarioRoles,
                    u => u.IdUsuario,
                    ur => ur.IdUsuario,
                    (u, ur) => new { Usuario = u, UsuarioRolId = ur.IdRol })
                .Join(_context.Roles,
                    combined => combined.UsuarioRolId,
                    r => r.IdRol,
                    (combined, r) => new { Usuario = combined.Usuario, Rol = r })
                .Where(combined => combined.Rol.Nombre.ToUpper() == "DOCTOR")
                .Select(combined => combined.Usuario)
                .ToListAsync();
        }
    }
}
