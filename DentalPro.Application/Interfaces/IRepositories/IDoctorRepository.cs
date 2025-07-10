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
        
        /// <summary>
        /// Obtiene los detalles de un doctor específico por su ID de usuario
        /// </summary>
        /// <param name="doctorId">ID del usuario con rol doctor</param>
        /// <returns>Detalles del doctor o null si no existe</returns>
        Task<DoctorDetail?> GetDoctorDetailByIdAsync(Guid doctorId);
        
        /// <summary>
        /// Obtiene todos los doctores con sus detalles en un consultorio específico
        /// </summary>
        /// <param name="consultorioId">ID del consultorio</param>
        /// <returns>Lista de usuarios doctores con sus detalles</returns>
        Task<IEnumerable<Usuario>> GetAllDoctorsWithDetailsAsync(Guid consultorioId);
        
        /// <summary>
        /// Guarda o actualiza los detalles de un doctor
        /// </summary>
        /// <param name="doctorId">ID del usuario con rol doctor</param>
        /// <param name="doctorDetail">Detalles del doctor a guardar</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        Task<bool> SaveDoctorDetailAsync(Guid doctorId, DoctorDetail doctorDetail);
        
        /// <summary>
        /// Elimina los detalles de un doctor
        /// </summary>
        /// <param name="doctorId">ID del usuario con rol doctor</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        Task<bool> DeleteDoctorDetailAsync(Guid doctorId);
    }
}
