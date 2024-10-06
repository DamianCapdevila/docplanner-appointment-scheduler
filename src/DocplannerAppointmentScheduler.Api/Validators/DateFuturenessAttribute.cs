using DocplannerAppointmentScheduler.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class DateFuturenessAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var request = (ScheduleAppointmentRequest)validationContext.ObjectInstance;
            if (request.Start <= DateTime.Now || request.End <= DateTime.Now)
            {
                return new ValidationResult("Start and End times must be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
