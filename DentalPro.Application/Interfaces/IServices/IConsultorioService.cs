using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de consultorios
/// </summary>
public interface IConsultorioService
{
    /// <summary>
    /// Obtiene todos los consultorios
    /// </summary>
    Task<IEnumerable<ConsultorioDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un consultorio por su ID
    /// </summary>
    Task<ConsultorioDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea un nuevo consultorio
    /// </summary>
    /// <param name="consultorioDto">DTO con los datos del consultorio a crear</param>
    /// <returns>DTO del consultorio creado</returns>
    Task<ConsultorioDto> CreateAsync(ConsultorioCreateDto consultorioDto);
    
    /// <summary>
    /// Actualiza un consultorio existente
    /// </summary>
    /// <param name="consultorioDto">DTO con los datos del consultorio a actualizar</param>
    /// <returns>DTO del consultorio actualizado</returns>
    Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto);
    
    /// <summary>
    /// Elimina un consultorio por su ID
    /// </summary>
    /// <param name="id">ID del consultorio a eliminar</param>
    /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByIdAsync(Guid id);
    
    /// <summary>
    /// Obtiene todos los doctores asociados a un consultorio específico
    /// </summary>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>Lista de doctores del consultorio</returns>
    Task<IEnumerable<UsuarioDto>> GetDoctoresByConsultorioAsync(Guid consultorioId);
    
    /// <summary>
    /// Obtiene todos los asistentes asociados a un consultorio específico
    /// </summary>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>Lista de asistentes del consultorio</returns>
    Task<IEnumerable<UsuarioDto>> GetAsistentesByConsultorioAsync(Guid consultorioId);
    
    /// <summary>
    /// Asigna un doctor a un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario doctor</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la asignación fue exitosa</returns>
    Task<bool> AsignarDoctorAsync(Guid usuarioId, Guid consultorioId);
    
    /// <summary>
    /// Asigna un asistente a un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario asistente</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la asignación fue exitosa</returns>
    Task<bool> AsignarAsistenteAsync(Guid usuarioId, Guid consultorioId);
    
    /// <summary>
    /// Desvincula un miembro del personal de un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario a desvincular</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la desvinculación fue exitosa</returns>
    Task<bool> DesvincularMiembroAsync(Guid usuarioId, Guid consultorioId);
}
