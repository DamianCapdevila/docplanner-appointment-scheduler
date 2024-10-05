using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Models
{
    public class ScheduleAppointmentRequest
    {
        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Required]
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
