using AutoMapper;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Consultorios
/// </summary>
public class ConsultorioProfile : Profile
{
    public ConsultorioProfile()
    {
        // Entity -> DTO
        CreateMap<Consultorio, ConsultorioDto>();
        
        // DTO -> Entity
        CreateMap<ConsultorioCreateDto, Consultorio>();
        CreateMap<ConsultorioUpdateDto, Consultorio>();
    }
}
