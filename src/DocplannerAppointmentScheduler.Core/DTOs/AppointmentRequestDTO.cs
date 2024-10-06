namespace DocplannerAppointmentScheduler.Core.DTOs
{
    /// <summary>
    /// Based on incoming data from our API, this class will be used for making a post request to the external availability service to take a slot.
    /// </summary>
    public class AppointmentRequestDTO
    {
        public FreeSlotDTO Slot { get; set; }

        public string FacilityId { get; set; }

        public string Comment { get; set; }
        public PatientDTO Patient { get; set; }
    }

    public class PatientDTO
    {
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
