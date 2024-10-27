using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Results;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public class SchedulerService : ISchedulerService
    {
        private IAvailabilityService _availabilityService;
        public SchedulerService(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }
        public async Task<Result<WeeklyAvailabilityDTO>> GetAvailableSlotsAsync(int weekNumber, int year)
        {
            return await _availabilityService.GetWeeklyAvailabilityAsync(weekNumber, year);
        }

        public async Task<Result<bool>> ScheduleAppointmentAsync(AppointmentRequestDTO appointmentRequest)
        {
            return await _availabilityService.TakeSlotAsync(appointmentRequest);
        }
    }
}
