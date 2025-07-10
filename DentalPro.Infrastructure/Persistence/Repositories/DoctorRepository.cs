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
        
        /// <summary>
        /// Obtiene los detalles de un doctor específico por su ID de usuario
        /// </summary>
        public async Task<DoctorDetail?> GetDoctorDetailByIdAsync(Guid doctorId)
        {
            // Primero verificamos que el usuario sea doctor
            if (!await IsUserDoctorAsync(doctorId))
                return null;
                
            return await _context.DoctorDetails
                .FirstOrDefaultAsync(d => d.IdUsuario == doctorId);
        }
        
        /// <summary>
        /// Obtiene todos los doctores con sus detalles en un consultorio específico
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetAllDoctorsWithDetailsAsync(Guid consultorioId)
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
                .Include(u => u.DoctorDetail)
                .ToListAsync();
        }
        
        /// <summary>
        /// Guarda o actualiza los detalles de un doctor
        /// </summary>
        public async Task<bool> SaveDoctorDetailAsync(Guid doctorId, DoctorDetail doctorDetail)
        {
            try
            {
                // Verificamos que el usuario sea doctor
                if (!await IsUserDoctorAsync(doctorId))
                    return false;
                    
                // Buscamos si ya existe un detalle para este doctor
                var existingDetail = await _context.DoctorDetails
                    .FirstOrDefaultAsync(d => d.IdUsuario == doctorId);
                    
                if (existingDetail == null)
                {
                    // Si no existe, creamos uno nuevo
                    doctorDetail.IdUsuario = doctorId;
                    await _context.DoctorDetails.AddAsync(doctorDetail);
                }
                else
                {
                    // Si existe, actualizamos sus propiedades
                    existingDetail.Especialidad = doctorDetail.Especialidad;
                    existingDetail.AñosExperiencia = doctorDetail.AñosExperiencia;
                    existingDetail.NumeroLicencia = doctorDetail.NumeroLicencia;
                    existingDetail.FechaGraduacion = doctorDetail.FechaGraduacion;
                    existingDetail.Certificaciones = doctorDetail.Certificaciones;
                    
                    _context.DoctorDetails.Update(existingDetail);
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Elimina los detalles de un doctor
        /// </summary>
        public async Task<bool> DeleteDoctorDetailAsync(Guid doctorId)
        {
            try
            {
                var doctorDetail = await _context.DoctorDetails
                    .FirstOrDefaultAsync(d => d.IdUsuario == doctorId);
                    
                if (doctorDetail == null)
                    return true; // No hay nada que eliminar
                    
                _context.DoctorDetails.Remove(doctorDetail);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
