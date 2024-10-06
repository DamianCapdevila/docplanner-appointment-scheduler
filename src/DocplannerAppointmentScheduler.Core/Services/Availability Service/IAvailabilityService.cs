using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface IAvailabilityService
    {
        Task<WeeklyAvailabilityDTO> GetWeeklyAvailabilityAsync(int weekNumber, int year);
        Task<bool> TakeSlotAsync(AppointmentRequestDTO request);
    }
}
