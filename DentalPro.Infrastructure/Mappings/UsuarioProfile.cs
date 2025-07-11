using AutoMapper;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Usuarios
/// </summary>
public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        // Entity -> DTO
        CreateMap<Usuario, UsuarioDto>();
        
        // DTO -> Entity
        CreateMap<UsuarioCreateDto, Usuario>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UltimoAcceso, opt => opt.Ignore())
            .ForMember(dest => dest.IntentosFallidos, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.BloqueoHasta, opt => opt.Ignore());
            
        CreateMap<UsuarioUpdateDto, Usuario>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.UltimoAcceso, opt => opt.Ignore())
            .ForMember(dest => dest.IntentosFallidos, opt => opt.Ignore())
            .ForMember(dest => dest.BloqueoHasta, opt => opt.Ignore());
    }
}
