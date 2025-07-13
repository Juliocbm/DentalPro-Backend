using DentalPro.Application.DTOs.Usuario;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IServices
{
    /// <summary>
    /// Servicio especializado en la gestión CRUD de usuarios.
    /// Maneja creación, lectura, actualización y eliminación de usuarios.
    /// </summary>
    public interface IUsuarioManagementService
    {
        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        Task<UsuarioDto?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        Task<UsuarioDto?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene todos los usuarios de un consultorio específico (usando int)
        /// </summary>
        Task<IEnumerable<UsuarioDto>> GetByConsultorioIdAsync(int consultorioId);
        
        /// <summary>
        /// Obtiene todos los usuarios de un consultorio específico (usando Guid)
        /// </summary>
        Task<IEnumerable<UsuarioDto>> GetByConsultorioGuidAsync(Guid consultorioId);

        /// <summary>
        /// Verifica si existe un usuario con el ID especificado
        /// </summary>
        Task<bool> ExistsByIdAsync(int id);

        /// <summary>
        /// Verifica si existe un usuario con el email especificado
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Verifica si existe un usuario con el email especificado, excepto el usuario actual
        /// </summary>
        Task<bool> ExistsByEmailExceptUserAsync(string email, int userId);

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        Task<UsuarioDto> CreateAsync(UsuarioCreateDto usuarioCreateDto);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        Task<UsuarioDto> UpdateAsync(int id, UsuarioUpdateDto usuarioUpdateDto);

        /// <summary>
        /// Elimina un usuario por su ID
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="changePasswordDto">DTO con contraseña actual y nueva</param>
        Task<bool> ChangePasswordAsync(int id, UsuarioChangePasswordDto changePasswordDto);
    }
}
