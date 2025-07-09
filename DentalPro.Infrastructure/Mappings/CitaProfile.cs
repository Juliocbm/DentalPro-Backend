using AutoMapper;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings;

/// <summary>
/// Configuración de mapeos AutoMapper para el módulo de Citas
/// </summary>
public class CitaProfile : Profile
{
    public CitaProfile()
    {
        // Entity -> DTO
        CreateMap<Cita, CitaDto>()
            .ForMember(dest => dest.NombrePaciente, opt => opt.MapFrom(src => $"{src.Paciente.Nombre} {src.Paciente.Apellidos}"))
            .ForMember(dest => dest.NombreDoctor, opt => opt.MapFrom(src => src.Doctor.Nombre));

        // Entity -> DetailDTO
        CreateMap<Cita, CitaDetailDto>()
            .ForMember(dest => dest.NombreCompletoPaciente, opt => opt.MapFrom(src => $"{src.Paciente.Nombre} {src.Paciente.Apellidos}"))
            .ForMember(dest => dest.EmailPaciente, opt => opt.MapFrom(src => src.Paciente.Correo))
            .ForMember(dest => dest.TelefonoPaciente, opt => opt.MapFrom(src => src.Paciente.Telefono))
            .ForMember(dest => dest.NombreCompletoDoctor, opt => opt.MapFrom(src => src.Doctor.Nombre))
            .ForMember(dest => dest.EmailDoctor, opt => opt.MapFrom(src => src.Doctor.Correo))
            .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => (int)(src.FechaHoraFin - src.FechaHoraInicio).TotalMinutes))
            .ForMember(dest => dest.TieneRecordatorios, opt => opt.MapFrom(src => src.Recordatorios.Any()));

        // DTO -> Entity
        CreateMap<CitaCreateDto, Cita>()
            .ForMember(dest => dest.IdCita, opt => opt.Ignore()) // ID generado por la BD
            .ForMember(dest => dest.Estatus, opt => opt.MapFrom(src => "Programada")) // Estado predeterminado
            .ForMember(dest => dest.Recordatorios, opt => opt.Ignore()) // Colección de navegación
            .ForMember(dest => dest.Paciente, opt => opt.Ignore()) // Propiedad de navegación
            .ForMember(dest => dest.Doctor, opt => opt.Ignore()); // Propiedad de navegación

        CreateMap<CitaUpdateDto, Cita>();
    }
}
