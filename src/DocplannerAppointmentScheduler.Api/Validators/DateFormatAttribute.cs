using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class DateFormatAttribute : ValidationAttribute
    {
        public DateFormatAttribute()
        {
            ErrorMessage = "Start and End dates must be in the format yyyy-MM-ddTHH:mm:ss.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateTime)
            {
                return new ValidationResult(ErrorMessage);
            }

            string formattedDate = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            if (formattedDate != dateTime.ToString("yyyy-MM-ddTHH:mm:ss"))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
