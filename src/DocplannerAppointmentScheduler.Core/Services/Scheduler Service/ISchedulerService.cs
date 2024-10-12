using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface ISchedulerService
    {
        public Task<HttpResponseMessage> GetAvailableSlotsAsync(int weekNumber, int year);
        public Task<HttpResponseMessage> ScheduleAppointmentAsync(AppointmentRequestDTO appointmentRequest);
    }
}
