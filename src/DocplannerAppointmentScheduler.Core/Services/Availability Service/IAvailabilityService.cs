using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface IAvailabilityService
    {
        Task<WeeklyAvailabilityResponseDTO> GetWeeklyAvailabilityAsync(DateTime weekStart);
        Task<bool> TakeSlotAsync(AppointmentRequestDTO request);
    }
}
