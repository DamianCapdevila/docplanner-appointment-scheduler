namespace DocplannerAppointmentScheduler.Domain
{
    public class WeeklyAvailability
    {
        private FacilityOccupancy _facilityOccupancy;
        public Facility Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
        public List<DaySchedule> DaySchedules { get; set; }

        public WeeklyAvailability(FacilityOccupancy facilityOccupancy)
        {
            _facilityOccupancy = facilityOccupancy;
            CalculateWeeklyAvailability();
        }

        private void CalculateWeeklyAvailability()
        {
            throw new NotImplementedException();
        }
    }

    public class DaySchedule
    {
        public DateTime Day { get; set; }
        public WorkPeriod WorkPeriod { get; set; }
        public List<FreeSlot> AvailableSlots { get; set; }
    }

    public class FreeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

    }

}


