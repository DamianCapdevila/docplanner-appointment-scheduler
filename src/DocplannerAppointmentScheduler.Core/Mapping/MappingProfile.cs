using AutoMapper;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Domain;

namespace DocplannerAppointmentScheduler.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping for FacilityOccupancy
            CreateMap<FacilityOccupancyDTO, FacilityOccupancy>()
                .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.Facility))
                .ForMember(dest => dest.SlotDurationMinutes, opt => opt.MapFrom(src => src.SlotDurationMinutes))
                .ForMember(dest => dest.Monday, opt => opt.MapFrom(src => src.Monday))
                .ForMember(dest => dest.Tuesday, opt => opt.MapFrom(src => src.Tuesday))
                .ForMember(dest => dest.Wednesday, opt => opt.MapFrom(src => src.Wednesday))
                .ForMember(dest => dest.Thursday, opt => opt.MapFrom(src => src.Thursday))
                .ForMember(dest => dest.Friday, opt => opt.MapFrom(src => src.Friday));

            CreateMap<FacilityDTO, Facility>();
            CreateMap<Facility, FacilityDTO>();  // Add reverse mapping

            CreateMap<DayOccupancyDTO, DayOccupancy>()
                .ForMember(dest => dest.WorkPeriod, opt => opt.MapFrom(src => src.WorkPeriod))
                .ForMember(dest => dest.BusySlots, opt => opt.MapFrom(src => src.BusySlots));

            CreateMap<WorkPeriodDTO, WorkPeriod>();
            CreateMap<WorkPeriod, WorkPeriodDTO>();  // Add reverse mapping

            CreateMap<BusySlotDTO, BusySlot>();
            CreateMap<BusySlot, BusySlotDTO>();  // Add reverse mapping

            // Mapping for WeeklyAvailability
            CreateMap<WeeklyAvailability, WeeklyAvailabilityDTO>()
                .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.Facility))
                .ForMember(dest => dest.SlotDurationMinutes, opt => opt.MapFrom(src => src.SlotDurationMinutes))
                .ForMember(dest => dest.DaySchedules, opt => opt.MapFrom(src => src.DaySchedules));

            CreateMap<WeeklyAvailabilityDTO, WeeklyAvailability>(); // Add reverse mapping for WeeklyAvailability

            CreateMap<DaySchedule, DayScheduleDTO>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Day.ToString()))  // Convert enum to string
                .ForMember(dest => dest.WorkPeriod, opt => opt.MapFrom(src => src.WorkPeriod))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.AvailableSlots));

            CreateMap<DayScheduleDTO, DaySchedule>(); // Add reverse mapping for DaySchedule

            CreateMap<FreeSlot, FreeSlotDTO>();
            CreateMap<FreeSlotDTO, FreeSlot>(); // Add reverse mapping for FreeSlot
        }
    }
}
