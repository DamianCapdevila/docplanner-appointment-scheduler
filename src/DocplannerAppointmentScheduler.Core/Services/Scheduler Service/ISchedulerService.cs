using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Results;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface ISchedulerService
    {
        public Task<Result<WeeklyAvailabilityDTO>> GetAvailableSlotsAsync(int weekNumber, int year);
        public Task<Result<bool>> ScheduleAppointmentAsync(AppointmentRequestDto appointmentRequest);
    }
}
