using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Core.DTOs
{ 
    public class AppointmentRequestDTO
    {
        
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid FacilityId { get; set; }
        public string? Comment { get; set; }
        public PatientDTO? Patient { get; set; }
    }

    public class PatientDTO
    {
        public string? Name { get; set; }
        public string? SecondName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
