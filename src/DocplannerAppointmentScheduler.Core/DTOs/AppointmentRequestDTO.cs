using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Core.DTOs
{ 
    public record AppointmentRequestDto
    {
        
        public required DateTime Start { get; init; }
        public required DateTime End { get; init; }
        public required Guid FacilityId { get; init; }
        public string? Comment { get; init; }
        public required PatientDto Patient { get; init; }
    }

    public record PatientDto
    {
        public required string Name { get; init; }
        public required string SecondName { get; init; }
        public required string Email { get; init; }
        public required string Phone { get; init; }
    }
}
