namespace DocplannerAppointmentScheduler.Domain
{
    public class AppointmentRequest
    {
        public TimeSlot Slot { get; set; }

        public string FacilityId { get; set; }

        public string Comment { get; set; }
        public Patient Patient { get; set; }
    }
}

