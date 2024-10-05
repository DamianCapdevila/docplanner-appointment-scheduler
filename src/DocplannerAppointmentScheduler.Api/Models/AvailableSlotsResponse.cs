namespace DocplannerAppointmentScheduler.Api.Models
{
    public class AvailableSlotsResponse
    {
        public List<SlotResponse> Slots { get; set; }
        public string FacilityId { get; set; }
        public DateTime Date { get; set; }
    }

    public class SlotResponse
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
