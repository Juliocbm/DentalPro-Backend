using AutoMapper;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Pacientes
/// </summary>
public class PacienteProfile : Profile
{
    public PacienteProfile()
    {
        // Entity -> DTO
        CreateMap<Paciente, PacienteDto>();
        
        // DTO -> Entity
        CreateMap<PacienteCreateDto, Paciente>();
        CreateMap<PacienteUpdateDto, Paciente>();
    }
}
