using DocplannerAppointmentScheduler.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DocplannerAppointmentScheduler.Api.Validators
{
    public class FutureWeekAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var request = (AvailableSlotsRequest)validationContext.ObjectInstance;
            var currentDate = DateTime.Now;
            var mondayInSelectedWeek = ISOWeek.ToDateTime(request.Year, request.WeekNumber, DayOfWeek.Monday);
            var sundayInSelectedWeek = mondayInSelectedWeek.AddDays(6);

            if (currentDate.Date > sundayInSelectedWeek)
            {
                return new ValidationResult("The selected week has already passed. Please choose a future week.");
            }

            return ValidationResult.Success;
        }
    }
}
