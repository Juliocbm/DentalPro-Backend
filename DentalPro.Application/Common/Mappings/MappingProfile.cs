using AutoMapper;
using DentalPro.Application.DTOs;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos de entidades y DTOs
            CreateMap<Rol, RolDto>().ReverseMap();
            
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            
            //CreateMap<Paciente, PacienteDto>().ReverseMap();
            
            // Puedes agregar más mapeos aquí para otras entidades
        }
    }
}
