using DentalPro.Application.DTOs.Citas;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio especializado para validación de citas
/// </summary>
public interface ICitaValidationService
{
    /// <summary>
    /// Valida una cita antes de crearla
    /// </summary>
    Task ValidateForCreateAsync(CitaCreateDto citaDto);
    
    /// <summary>
    /// Valida una cita antes de actualizarla
    /// </summary>
    Task ValidateForUpdateAsync(CitaUpdateDto citaDto, Cita citaExistente);
    
    /// <summary>
    /// Valida una cita antes de cancelarla
    /// </summary>
    Task ValidateForCancelAsync(Guid idCita);
    
    /// <summary>
    /// Valida una cita antes de eliminarla
    /// </summary>
    Task ValidateForDeleteAsync(Guid idCita);
    
    /// <summary>
    /// Verifica si hay traslapes para un horario de cita
    /// </summary>
    Task<bool> HasOverlappingAppointmentsAsync(Guid idDoctor, DateTime fechaHoraInicio, DateTime fechaHoraFin, Guid? idCitaExcluir = null);
    
    /// <summary>
    /// Verifica si un usuario es un doctor
    /// </summary>
    Task<bool> IsUserDoctorAsync(Guid idUsuario);
    
    /// <summary>
    /// Obtiene los detalles del doctor por su ID
    /// </summary>
    Task<DoctorDetail> GetDoctorDetailAsync(Guid idDoctor);
}
