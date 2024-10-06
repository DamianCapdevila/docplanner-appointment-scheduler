namespace DocplannerAppointmentScheduler.Core.DTOs
{
    /// <summary>
    /// Based on the calculations done in the Domain layer, this class will be used to talk to our API and present the available time slots.
    /// </summary>
    public class WeeklyAvailabilityDTO
    {
        public FacilityDTO Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
        public List<DayScheduleDTO> DaySchedules { get; set; }

    }

    public class DayScheduleDTO
    {
        public DateTime Day { get; set; }
        public WorkPeriodDTO WorkPeriod { get; set; }
        public List<FreeSlotDTO> AvailableSlots { get; set; }
    }

    public class FreeSlotDTO
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
