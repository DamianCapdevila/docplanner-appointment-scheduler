using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Results;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface IAvailabilityService
    {
        Task<Result<WeeklyAvailabilityDTO>> GetWeeklyAvailabilityAsync(int weekNumber, int year);
        Task<Result<bool>> TakeSlotAsync(AppointmentRequestDTO request);
    }
}
