using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public interface IAvailabilityService
    {
        Task<HttpResponseMessage> GetWeeklyAvailabilityAsync(int weekNumber, int year);
        Task<HttpResponseMessage> TakeSlotAsync(AppointmentRequestDTO request);
    }
}
