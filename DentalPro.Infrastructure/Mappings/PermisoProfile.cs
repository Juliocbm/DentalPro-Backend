using AutoMapper;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapear entre la entidad Permiso y sus DTOs
/// </summary>
public class PermisoProfile : Profile
{
    /// <summary>
    /// Constructor que configura los mapeos
    /// </summary>
    public PermisoProfile()
    {
        // Entidad -> DTOs
        CreateMap<Permiso, PermisoDto>()
            .ForMember(dest => dest.IdPermiso, opt => opt.MapFrom(src => src.IdPermiso))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion));
        
        CreateMap<Permiso, PermisoDetailDto>()
            .ForMember(dest => dest.IdPermiso, opt => opt.MapFrom(src => src.IdPermiso))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
        
        // DTOs -> Entidad
        CreateMap<PermisoCreateDto, Permiso>()
            .ForMember(dest => dest.IdPermiso, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.Modulo, opt => opt.Ignore())
            .ForMember(dest => dest.EsOperacion, opt => opt.Ignore())
            .ForMember(dest => dest.PredeterminadoSistema, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.Usuarios, opt => opt.Ignore());
        
        CreateMap<PermisoUpdateDto, Permiso>()
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.Modulo, opt => opt.Ignore())
            .ForMember(dest => dest.EsOperacion, opt => opt.Ignore())
            .ForMember(dest => dest.PredeterminadoSistema, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.Usuarios, opt => opt.Ignore());
    }
}
