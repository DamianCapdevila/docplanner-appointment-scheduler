using DocplannerAppointmentScheduler.Api.Validators;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DocplannerAppointmentScheduler.Api.Models
{
    public class AvailableSlotsRequest
    {
        [Range(1, 53, ErrorMessage = "The week number must be between 1 and 53.")]
        [FutureWeek(ErrorMessage = "The selected week has already passed. Please choose a future week.")]
        public int WeekNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The year must be a positive number.")]
        public int Year { get; set; }
    }
}
