using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories;

/// <summary>
/// Interfaz para el repositorio de citas
/// </summary>
public interface ICitaRepository
{
    /// <summary>
    /// Obtiene todas las citas
    /// </summary>
    Task<IEnumerable<Cita>> GetAllAsync();

    /// <summary>
    /// Obtiene todas las citas de un consultorio
    /// </summary>
    Task<IEnumerable<Cita>> GetByConsultorioAsync(Guid idConsultorio);
    
    /// <summary>
    /// Obtiene todas las citas de un paciente
    /// </summary>
    Task<IEnumerable<Cita>> GetByPacienteAsync(Guid idPaciente);
    
    /// <summary>
    /// Obtiene todas las citas de un doctor
    /// </summary>
    Task<IEnumerable<Cita>> GetByDoctorAsync(Guid idDoctor);
    
    /// <summary>
    /// Obtiene todas las citas por rango de fechas
    /// </summary>
    Task<IEnumerable<Cita>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin, Guid idConsultorio);
    
    /// <summary>
    /// Obtiene una cita por su ID
    /// </summary>
    Task<Cita?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe una cita con el ID especificado
    /// </summary>
    Task<bool> ExistsByIdAsync(Guid id);
    
    /// <summary>
    /// Verifica si existen citas traslapadas para un doctor en un rango de fechas
    /// </summary>
    Task<bool> HasOverlappingAppointmentsAsync(Guid idDoctor, DateTime fechaInicio, DateTime fechaFin, Guid? idCitaExcluir = null);
    
    /// <summary>
    /// Agrega una nueva cita
    /// </summary>
    Task<Cita> AddAsync(Cita cita);
    
    /// <summary>
    /// Actualiza una cita existente
    /// </summary>
    Task<Cita> UpdateAsync(Cita cita);
    
    /// <summary>
    /// Elimina una cita
    /// </summary>
    Task DeleteAsync(Guid id);
}
