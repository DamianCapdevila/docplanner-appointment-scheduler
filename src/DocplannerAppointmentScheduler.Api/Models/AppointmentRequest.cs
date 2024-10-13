using DocplannerAppointmentScheduler.Api.Validators;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Models
{

    [DateFutureness(ErrorMessage = "Start and End times must be in the future.")]
    [DateRange(ErrorMessage = "Start time must be earlier than End time.")]
    public class AppointmentRequest
    {
        [DateFormat(ErrorMessage = "Start time must be in the format yyyy-MM-ddTHH:mm:ss.")]
        [Required(ErrorMessage = "Start time is required")]
        public DateTime Start { get; set; }

        [DateFormat(ErrorMessage = "End time must be in the format yyyy-MM-ddTHH:mm:ss.")]
        [Required(ErrorMessage = "End time is required")]
        public DateTime End { get; set; }

        [Required(ErrorMessage = "FacilityId is required")]
        public Guid FacilityId { get; set; }

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
