using DentalPro.Application.DTOs.Citas;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio de citas
/// </summary>
public interface ICitaService
{
    /// <summary>
    /// Obtiene todas las citas del consultorio del usuario actual
    /// </summary>
    Task<IEnumerable<CitaDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene todas las citas en un rango de fechas
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene todas las citas de un paciente específico
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByPacienteAsync(Guid idPaciente);
    
    /// <summary>
    /// Obtiene todas las citas de un usuario (doctor) específico
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByUsuarioAsync(Guid idUsuario);
    
    /// <summary>
    /// Obtiene una cita por su ID
    /// </summary>
    Task<CitaDetailDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea una nueva cita
    /// </summary>
    Task<CitaDto> CreateAsync(CitaCreateDto citaDto, Guid idUsuarioActual);
    
    /// <summary>
    /// Actualiza una cita existente
    /// </summary>
    Task<CitaDto> UpdateAsync(CitaUpdateDto citaDto);
    
    /// <summary>
    /// Cancela una cita
    /// </summary>
    Task CancelAsync(Guid id);
    
    /// <summary>
    /// Elimina una cita
    /// </summary>
    Task DeleteAsync(Guid id);
}
