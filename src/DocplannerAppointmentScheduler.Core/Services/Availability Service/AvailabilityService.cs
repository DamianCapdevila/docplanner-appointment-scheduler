using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DocplannerAppointmentScheduler.Core.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private HttpClient _httpClient;
        public AvailabilityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
        }
        public async Task<WeeklyAvailabilityResponseDTO> GetWeeklyAvailabilityAsync(DateTime date)
        {
            // Select the closest monday within the date the user selected.
            //Todo: Implement

            // Format the date as yyyyMMdd before sending it to the external API
            string formattedDate = date.ToString("yyyyMMdd");

            // Call the external API with the formatted date (mocked here)
            var response = await _httpClient.GetAsync("https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/" + formattedDate);

            // Example deserialized response (mocked for demonstration)
            var weeklyAvailability = new WeeklyAvailabilityResponseDTO
            {
                Facility = new Facility { /* Facility details here */ },
                SlotDurationMinutes = 30, // Assume each slot is 30 minutes long
                Monday = new DaySchedule
                {
                    WorkPeriod = new WorkPeriod
                    {
                        StartHour = 9,
                        EndHour = 17,
                        LunchStartHour = 12,
                        LunchEndHour = 13
                    },
                    BusySlots = new List<TimeSlot> 
                    {
                        new TimeSlot {Start = date, End = date},
                        new TimeSlot {Start = date,End = date}
                    }
                   
                }
                
            };
            return weeklyAvailability;
        }

        public Task<bool> TakeSlotAsync(AppointmentRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
