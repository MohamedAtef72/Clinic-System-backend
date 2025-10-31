using AutoMapper;
using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;

namespace Clinic_System.Infrastructure.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Appointment, AppointmentDTO>()
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Availability.DoctorId));
            CreateMap<AppointmentDTO, Appointment>();
            CreateMap<Visit, VisitReadDTO>();
            CreateMap<VisitCreateDTO, Visit>();
            CreateMap<VisitReadDTO, Visit>();

        }
    }

}
