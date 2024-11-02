using DocplannerAppointmentScheduler.Api.Validators;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Models
{
    public record AvailableSlotsRequest
    {
        [Required]
        [Range(1, 53, ErrorMessage = "The week number must be between 1 and 53.")]
        [FutureWeek(ErrorMessage = "The selected week has already passed. Please choose a future week.")]
        public required int WeekNumber { get; init; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The year must be a positive number.")]
        public required int Year { get; init; }
    }
}
