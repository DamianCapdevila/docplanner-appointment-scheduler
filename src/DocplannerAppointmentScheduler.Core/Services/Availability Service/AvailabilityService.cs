using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        public Task<WeeklyAvailabilityResponseDTO> GetWeeklyAvailabilityAsync(DateTime weekStart)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TakeSlotAsync(AppointmentRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
