using AutoMapper;
using DentalPro.Application.DTOs;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Application.DTOs.Citas;
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
            CreateMap<UsuarioRol, RolDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdRol))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Nombre : string.Empty));
            
            // Pacientes
            CreateMap<Paciente, PacienteDto>();
            CreateMap<PacienteCreateDto, Paciente>();
            CreateMap<PacienteUpdateDto, Paciente>();
            
            // Citas
            CreateMap<Cita, CitaDto>();
            CreateMap<CitaCreateDto, Cita>()
                .ForMember(dest => dest.IdCita, opt => opt.Ignore()) // ID generado por la BD
                .ForMember(dest => dest.Estatus, opt => opt.MapFrom(src => "Programada")) // Estado predeterminado
                .ForMember(dest => dest.Recordatorios, opt => opt.Ignore()) // Colección de navegación
                .ForMember(dest => dest.Paciente, opt => opt.Ignore()) // Propiedad de navegación
                .ForMember(dest => dest.Usuario, opt => opt.Ignore()); // Propiedad de navegación
            
            // Puedes agregar más mapeos aquí para otras entidades
        }
    }
}
