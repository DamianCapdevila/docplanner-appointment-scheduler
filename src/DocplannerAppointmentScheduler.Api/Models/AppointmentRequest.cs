using DocplannerAppointmentScheduler.Api.Validators;
using DocplannerAppointmentScheduler.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace DocplannerAppointmentScheduler.Api.Models
{

    [DateFutureness(ErrorMessage = "Start and End times must be in the future.")]
    [DateRange(ErrorMessage = "Start time must be earlier than End time.")]
    public record AppointmentRequest
    {
        [DateFormat(ErrorMessage = "Start time must be in the format yyyy-MM-ddTHH:mm:ss.")]
        [Required(ErrorMessage = "Start time is required")]
        public required DateTime Start { get; init; }

        [DateFormat(ErrorMessage = "End time must be in the format yyyy-MM-ddTHH:mm:ss.")]
        [Required(ErrorMessage = "End time is required")]
        public required DateTime End { get; init; }

        [Required(ErrorMessage = "FacilityId is required")]
        public required Guid FacilityId { get; init; }
        
        [StringLength(100)]
        public string? Comment { get; init; }

        [Required]
        public required Patient Patient { get; init; }
        
        
        public static implicit operator AppointmentRequestDto(AppointmentRequest appointmentRequest)
        {
            return new AppointmentRequestDto()
            {
                Start = appointmentRequest.Start,
                End = appointmentRequest.End,
                FacilityId = appointmentRequest.FacilityId,
                Comment = appointmentRequest.Comment,
                Patient = appointmentRequest.Patient,
            };
        }
    }

    public record Patient
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; init; }

        [Required]
        [StringLength(100)]
        public required string SecondName { get; init; }

        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        [Required]
        [Phone]
        public required string Phone { get; init; }

        public static implicit operator PatientDto(Patient patient)
        {
            return new PatientDto()
            {
                Name = patient.Name,
                SecondName = patient.SecondName,
                Email = patient.Email,
                Phone = patient.Phone
            };
        }
        
    }
}
