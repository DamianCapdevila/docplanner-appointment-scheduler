using AutoMapper;
using DocplannerAppointmentScheduler.Api.Models;
using DocplannerAppointmentScheduler.Core.DTOs;
namespace DocplannerAppointmentScheduler.Api.Mappers
{
    public class ApiAndCoreMapper : Profile
    {
        public ApiAndCoreMapper()
        {
            // Mapping for AppointmentRequest to AppointmentRequestDTO
            CreateMap<AppointmentRequest, AppointmentRequestDTO>()
            .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Start))
            .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.End))
            .ForMember(dest => dest.FacilityId, opt => opt.MapFrom(src => src.FacilityId))
            .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => new PatientDTO
            {
                Name = src.PatientRequest.Name,
                SecondName = src.PatientRequest.SecondName,
                Email = src.PatientRequest.Email,
                Phone = src.PatientRequest.Phone
            }));
        }
    }
}
