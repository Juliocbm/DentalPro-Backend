using AutoMapper;
using DentalPro.Application.DTOs.Audit;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Mappings
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore()); // El nombre de usuario se llenará con un resolver específico
        }
    }
}
