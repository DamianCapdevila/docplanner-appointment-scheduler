using DocplannerAppointmentScheduler.Api.Validators;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Models
{
    [DateRange(ErrorMessage = "Start time must be earlier than End time.")]
    public class ScheduleAppointmentRequest
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime End { get; set; }

        [Required(ErrorMessage = "FacilityId is required")]
        public string FacilityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Comment { get; set; }

        [Required]
        public PatientRequest PatientRequest { get; set; }
    }

    public class PatientRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string SecondName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
        
    }
}
