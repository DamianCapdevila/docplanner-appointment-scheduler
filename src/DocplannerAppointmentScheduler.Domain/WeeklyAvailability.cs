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
            SlotDurationMinutes = facilityOccupancy.SlotDurationMinutes;
            Facility = facilityOccupancy.Facility;
            DaySchedules = new List<DaySchedule>();
            CalculateWeeklyAvailability();
        }

        private void CalculateWeeklyAvailability()
        {
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Monday, _facilityOccupancy.Monday));
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Tuesday, _facilityOccupancy.Tuesday));
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Wednesday, _facilityOccupancy.Wednesday));
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Thursday, _facilityOccupancy.Thursday));
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Friday, _facilityOccupancy.Friday));
        }

        private DaySchedule CalculateDayAvailability(DayOfWeek day, DayOccupancy dayOccupancy)
        {
            // If dayOccupancy is null, it means no availability is filled for this day, so return a DaySchedule with no slots
            if (dayOccupancy == null)
            {
                return new DaySchedule
                {
                    Day = day, 
                    AvailableSlots = new List<FreeSlot>() 
                };
            }

            var workPeriod = dayOccupancy.WorkPeriod;
            var availableSlots = new List<FreeSlot>();

            var startTime = DateTime.Today.AddHours(workPeriod.StartHour);
            var endTime = DateTime.Today.AddHours(workPeriod.EndHour);
            var lunchStart = DateTime.Today.AddHours(workPeriod.LunchStartHour);
            var lunchEnd = DateTime.Today.AddHours(workPeriod.LunchEndHour);

            // Iterate from start to end, skipping busy slots
            while (startTime < endTime)
            {
                // Skip lunch break
                if (startTime >= lunchStart && startTime < lunchEnd)
                {
                    startTime = lunchEnd;
                    continue;
                }

                var slotEnd = startTime.AddMinutes(SlotDurationMinutes);

                // Check if the slot overlaps with any busy slots
                bool isBusy = dayOccupancy.BusySlots.Any(busySlot =>
                    (startTime >= busySlot.Start && startTime < busySlot.End) ||
                    (slotEnd > busySlot.Start && slotEnd <= busySlot.End));

                if (!isBusy)
                {
                    availableSlots.Add(new FreeSlot
                    {
                        Start = startTime,
                        End = slotEnd
                    });
                }

                startTime = slotEnd; // Move to the next slot
            }

            return new DaySchedule
            {
                Day = day, 
                WorkPeriod = workPeriod,
                AvailableSlots = availableSlots
            };
        }

    }

    public class DaySchedule
    {
        public DayOfWeek Day { get; set; }
        public WorkPeriod WorkPeriod { get; set; }
        public List<FreeSlot> AvailableSlots { get; set; }
    }

    public class FreeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

    }

}


