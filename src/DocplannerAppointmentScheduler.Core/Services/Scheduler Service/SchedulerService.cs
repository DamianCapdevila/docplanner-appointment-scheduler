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
        public async Task<WeeklyAvailabilityDTO> GetAvailableSlotsAsync(int weekNumber, int year)
        {
            return await _availabilityService.GetWeeklyAvailabilityAsync(weekNumber, year);
        }

        public async Task<HttpResponseMessage> ScheduleAppointmentAsync(AppointmentRequestDTO appointmentRequest)
        {
            return await _availabilityService.TakeSlotAsync(appointmentRequest);
        }
    }
}
