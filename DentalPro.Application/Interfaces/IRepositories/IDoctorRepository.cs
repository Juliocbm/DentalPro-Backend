using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Repositorio específico para operaciones relacionadas con doctores (usuarios con rol de Doctor)
    /// </summary>
    public interface IDoctorRepository
    {
        /// <summary>
        /// Verifica si un usuario tiene el rol de Doctor
        /// </summary>
        Task<bool> IsUserDoctorAsync(Guid userId);
        
        /// <summary>
        /// Obtiene todos los usuarios con rol de Doctor en un consultorio específico
        /// </summary>
        Task<IEnumerable<Usuario>> GetAllDoctorsAsync(Guid consultorioId);
    }
}
