using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface ISchedulerService
    {
        public Task<WeeklyAvailabilityDTO> GetAvailableSlots(int weekNumber, int year);
        public Task<bool> ScheduleAppointment(AppointmentRequestDTO appointmentRequest);
    }
}
