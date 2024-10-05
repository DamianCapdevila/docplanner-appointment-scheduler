using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface ISchedulerService
    {
        public Task<List<TimeSlotDTO>> GetAvailableSlots(DateTime date);
        public Task<bool> ScheduleAppointment(AppointmentRequestDTO appointmentRequest);
    }
}
