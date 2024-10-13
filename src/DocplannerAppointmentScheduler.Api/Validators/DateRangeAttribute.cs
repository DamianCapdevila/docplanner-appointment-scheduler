using DocplannerAppointmentScheduler.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is AppointmentRequest request)
            {
                if (request.Start >= request.End)
                {
                    return new ValidationResult("Start time must be earlier than End time.");
                }
            }
            else
            {
                return new ValidationResult("Invalid object instance. Start/End date are not in the format yyyy-MM-ddTHH:mm:ss");
            }

            return ValidationResult.Success;
        }
    }
}
