using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public class SchedulerService : ISchedulerService
    {
        private IAvailabilityService _availabilityService;
        public SchedulerService(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }
        public Task<WeeklyAvailabilityDTO> GetAvailableSlots(int weekNumber, int year)
        {
            return _availabilityService.GetWeeklyAvailabilityAsync(weekNumber, year);
        }

        public Task<bool> ScheduleAppointment(AppointmentRequestDTO appointmentRequest)
        {
            throw new NotImplementedException();
        }
    }
}
