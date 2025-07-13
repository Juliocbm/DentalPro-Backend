using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.DTOs.Usuario;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio especializado para la gestión del personal de un consultorio
/// </summary>
public interface IConsultorioStaffService
{
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
