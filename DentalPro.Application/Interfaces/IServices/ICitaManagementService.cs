using DentalPro.Application.DTOs.Citas;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio especializado para operaciones CRUD de citas
/// </summary>
public interface ICitaManagementService
{
    /// <summary>
    /// Obtiene todas las citas del consultorio del usuario actual
    /// </summary>
    Task<IEnumerable<CitaDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene todas las citas en un rango de fechas
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin, Guid idConsultorio);
    
    /// <summary>
    /// Obtiene todas las citas de un paciente específico
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByPacienteAsync(Guid idPaciente);
    
    /// <summary>
    /// Obtiene todas las citas de un doctor específico
    /// </summary>
    Task<IEnumerable<CitaDto>> GetByDoctorAsync(Guid idDoctor, Guid idConsultorio);
    
    /// <summary>
    /// Obtiene una cita por su ID
    /// </summary>
    Task<CitaDetailDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea una nueva cita en la base de datos
    /// </summary>
    Task<CitaDto> CreateAsync(Cita cita);
    
    /// <summary>
    /// Actualiza una cita existente en la base de datos
    /// </summary>
    Task<CitaDto> UpdateAsync(Cita cita);
    
    /// <summary>
    /// Cancela una cita
    /// </summary>
    Task CancelAsync(Guid id);
    
    /// <summary>
    /// Elimina una cita
    /// </summary>
    Task DeleteAsync(Guid id);
}
