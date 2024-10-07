namespace DocplannerAppointmentScheduler.Domain
{
    public class WeeklyAvailability
    {
        private FacilityOccupancy _facilityOccupancy;
        private int _slotDurationMinutes; 

        public Facility Facility { get; set; }
        public List<DaySchedule> DaySchedules { get; set; }

        public WeeklyAvailability(FacilityOccupancy facilityOccupancy)
        {
            _facilityOccupancy = facilityOccupancy;
            _slotDurationMinutes = facilityOccupancy.SlotDurationMinutes; 
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
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Saturday, _facilityOccupancy.Saturday)); 
            DaySchedules.Add(CalculateDayAvailability(DayOfWeek.Sunday, _facilityOccupancy.Sunday)); 
        }

        private DaySchedule CalculateDayAvailability(DayOfWeek day, DayOccupancy dayOccupancy)
        {
            if (dayOccupancy == null || dayOccupancy.WorkPeriod == null)
            {
                return new DaySchedule
                {
                    Day = day.ToString(),
                    AvailableSlots = new List<FreeSlot>()
                };
            }

            var workPeriod = dayOccupancy.WorkPeriod;
            var availableSlots = new List<FreeSlot>();

            var startTime = DateTime.Today.AddHours(workPeriod.StartHour);
            var endTime = DateTime.Today.AddHours(workPeriod.EndHour);
            var lunchStart = DateTime.Today.AddHours(workPeriod.LunchStartHour);
            var lunchEnd = DateTime.Today.AddHours(workPeriod.LunchEndHour);

            while (startTime < endTime)
            {
                if (startTime >= lunchStart && startTime < lunchEnd)
                {
                    startTime = lunchEnd;
                    continue;
                }

                var slotEnd = startTime.AddMinutes(_slotDurationMinutes); 

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

                startTime = slotEnd; 
            }

            return new DaySchedule
            {
                Day = day.ToString(),
                AvailableSlots = availableSlots
            };
        }
    }

    public class DaySchedule
    {
        public string Day { get; set; }
        public List<FreeSlot> AvailableSlots { get; set; }
    }

    public class FreeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
