using AutoMapper;
using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Recordatorios
/// </summary>
public class RecordatorioProfile : Profile
{
    public RecordatorioProfile()
    {
        // Entity -> DTO
        CreateMap<Recordatorio, RecordatorioDto>();
        
        // DTO -> Entity
        CreateMap<RecordatorioCreateDto, Recordatorio>()
            .ForMember(dest => dest.IdRecordatorio, opt => opt.Ignore())
            .ForMember(dest => dest.Enviado, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.FechaEnvio, opt => opt.Ignore())
            .ForMember(dest => dest.Cita, opt => opt.Ignore());

        CreateMap<RecordatorioUpdateDto, Recordatorio>()
            .ForMember(dest => dest.Enviado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEnvio, opt => opt.Ignore())
            .ForMember(dest => dest.Cita, opt => opt.Ignore());
    }
}
