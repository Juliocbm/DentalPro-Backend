using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para operaciones CRUD de citas
/// </summary>
public class CitaManagementService : ICitaManagementService
{
    private readonly ICitaRepository _citaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CitaManagementService> _logger;

    public CitaManagementService(
        ICitaRepository citaRepository,
        IMapper mapper,
        ILogger<CitaManagementService> logger)
    {
        _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CitaDto>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todas las citas");
        
        var citas = await _citaRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin, Guid idConsultorio)
    {
        _logger.LogInformation("Obteniendo citas por rango de fechas: {FechaInicio} - {FechaFin} para consultorio {IdConsultorio}",
            fechaInicio, fechaFin, idConsultorio);
        
        var citas = await _citaRepository.GetByDateRangeAsync(fechaInicio, fechaFin, idConsultorio);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByPacienteAsync(Guid idPaciente)
    {
        _logger.LogInformation("Obteniendo citas para el paciente con ID {IdPaciente}", idPaciente);
        
        var citas = await _citaRepository.GetByPacienteAsync(idPaciente);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByDoctorAsync(Guid idDoctor, Guid idConsultorio)
    {
        _logger.LogInformation("Obteniendo citas para el doctor con ID {IdDoctor} en consultorio {IdConsultorio}", 
            idDoctor, idConsultorio);
        
        var citas = await _citaRepository.GetByDoctorAsync(idDoctor);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<CitaDetailDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Obteniendo detalles de cita con ID {IdCita}", id);
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            _logger.LogWarning("No se encontró la cita con ID {IdCita}", id);
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        return _mapper.Map<CitaDetailDto>(cita);
    }

    public async Task<CitaDto> CreateAsync(Cita cita)
    {
        _logger.LogInformation("Creando nueva cita para paciente {IdPaciente} con doctor {IdDoctor}", 
            cita.IdPaciente, cita.IdDoctor);

        var citaCreada = await _citaRepository.AddAsync(cita);
        
        _logger.LogInformation("Cita creada con ID {IdCita}", citaCreada.IdCita);
        return _mapper.Map<CitaDto>(citaCreada);
    }

    public async Task<CitaDto> UpdateAsync(Cita cita)
    {
        _logger.LogInformation("Actualizando cita con ID {IdCita}", cita.IdCita);

        var citaActualizada = await _citaRepository.UpdateAsync(cita);
        
        _logger.LogInformation("Cita actualizada con ID {IdCita}", citaActualizada.IdCita);
        return _mapper.Map<CitaDto>(citaActualizada);
    }

    public async Task CancelAsync(Guid id)
    {
        _logger.LogInformation("Cancelando cita con ID {IdCita}", id);
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            _logger.LogWarning("No se encontró la cita con ID {IdCita} para cancelar", id);
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        cita.Estatus = "Cancelada";
        await _citaRepository.UpdateAsync(cita);
        
        _logger.LogInformation("Cita cancelada con ID {IdCita}", id);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Eliminando cita con ID {IdCita}", id);
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            _logger.LogWarning("No se encontró la cita con ID {IdCita} para eliminar", id);
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        await _citaRepository.DeleteAsync(id);
        
        _logger.LogInformation("Cita eliminada con ID {IdCita}", id);
    }
}
