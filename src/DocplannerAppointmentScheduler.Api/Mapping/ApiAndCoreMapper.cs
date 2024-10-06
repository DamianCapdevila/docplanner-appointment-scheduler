using AutoMapper;
using DocplannerAppointmentScheduler.Api.Models;
using DocplannerAppointmentScheduler.Core.DTOs;
namespace DocplannerAppointmentScheduler.Api.Mapping
{ 
    public class ApiAndCoreMapper : Profile
    {
        public ApiAndCoreMapper()
        {
            // Mapping for AppointmentRequest to AppointmentRequestDTO
            CreateMap<AppointmentRequest, AppointmentRequestDTO>()
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => new FreeSlotDTO
                {
                    Start = src.Start,
                    End = src.End
                }))
                .ForMember(dest => dest.FacilityId, opt => opt.MapFrom(src => src.FacilityId))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
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
