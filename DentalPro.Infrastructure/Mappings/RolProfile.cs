using AutoMapper;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Roles
/// </summary>
public class RolProfile : Profile
{
    public RolProfile()
    {
        // Entity -> DTO
        CreateMap<Rol, RolDto>();
        
        // DTO -> Entity
        CreateMap<RolCreateDto, Rol>();
        CreateMap<RolUpdateDto, Rol>();
        
        // Relación Usuario-Rol
        CreateMap<UsuarioRol, RolDetailDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdRol))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Nombre : string.Empty));
    }
}
