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
        CreateMap<UsuarioCreateDto, Usuario>();
        CreateMap<UsuarioUpdateDto, Usuario>();
    }
}
