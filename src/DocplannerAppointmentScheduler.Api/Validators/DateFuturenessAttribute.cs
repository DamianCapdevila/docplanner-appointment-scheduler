using DocplannerAppointmentScheduler.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class DateFuturenessAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is not AppointmentRequest request)
            {
                return new ValidationResult("Invalid object instance. Start/End date are not in the format yyyy-MM-ddTHH:mm:ss");
            }

            if (request.Start <= DateTime.UtcNow || request.End <= DateTime.UtcNow)
            {
                return new ValidationResult("Start and End times must be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
