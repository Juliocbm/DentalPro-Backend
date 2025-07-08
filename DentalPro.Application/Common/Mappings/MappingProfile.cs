using AutoMapper;
using DentalPro.Application.DTOs;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos de entidades y DTOs - Enfoque unificado
            // Roles
            CreateMap<Rol, RolDto>();
            CreateMap<RolCreateDto, Rol>();
            CreateMap<RolUpdateDto, Rol>();
            
            // Usuarios
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<UsuarioCreateDto, Usuario>();
            CreateMap<UsuarioUpdateDto, Usuario>();
            
            // Relación Usuario-Rol
            CreateMap<UsuarioRol, RolInfoDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdRol))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Nombre : string.Empty));
            
            // Pacientes
            CreateMap<Paciente, PacienteDto>();
            CreateMap<PacienteCreateDto, Paciente>();
            CreateMap<PacienteUpdateDto, Paciente>();
            
            // Puedes agregar más mapeos aquí para otras entidades
        }
    }
}
