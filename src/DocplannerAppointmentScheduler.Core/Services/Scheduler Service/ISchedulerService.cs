using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface ISchedulerService
    {
        public Task<WeeklyAvailabilityDTO> GetAvailableSlotsAsync(int weekNumber, int year);
        public Task<bool> ScheduleAppointmentAsync(AppointmentRequestDTO appointmentRequest);
    }
}
