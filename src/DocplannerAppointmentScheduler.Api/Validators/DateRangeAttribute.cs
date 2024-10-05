using DocplannerAppointmentScheduler.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var request = (ScheduleAppointmentRequest)validationContext.ObjectInstance;
            if (request.Start >= request.End)
            {
                return new ValidationResult("Start time must be earlier than End time.");
            }
            return ValidationResult.Success;
        }
    }
}
