using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public class SchedulerService : ISchedulerService
    {
        public Task<List<TimeSlotDTO>> GetAvailableSlots(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ScheduleAppointment(AppointmentRequestDTO appointmentRequest)
        {
            throw new NotImplementedException();
        }
    }
}
