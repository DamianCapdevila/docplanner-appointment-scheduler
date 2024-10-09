namespace DocplannerAppointmentScheduler.Domain
{
    public class WeeklyAvailability
    {
        private FacilityOccupancy _facilityOccupancy;
        private int _slotDurationMinutes;
        private DateTimeKind _dateTimeKind;

        public Facility Facility { get; set; }
        public List<DaySchedule> DaySchedules { get; set; }

        public WeeklyAvailability(FacilityOccupancy facilityOccupancy)
        {
            _facilityOccupancy = facilityOccupancy;
            _slotDurationMinutes = facilityOccupancy.SlotDurationMinutes;
            _dateTimeKind = DateTimeKind.Unspecified;
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

            var startTime = DateTime.SpecifyKind(DateTime.Today.AddHours(workPeriod.StartHour), _dateTimeKind);
            var endTime = DateTime.SpecifyKind(DateTime.Today.AddHours(workPeriod.EndHour), _dateTimeKind);
            var lunchStart = DateTime.SpecifyKind(DateTime.Today.AddHours(workPeriod.LunchStartHour), _dateTimeKind);
            var lunchEnd = DateTime.SpecifyKind(DateTime.Today.AddHours(workPeriod.LunchEndHour), _dateTimeKind);

            while (startTime < endTime)
            {
                if (startTime >= lunchStart && startTime < lunchEnd)
                {
                    startTime = lunchEnd;
                    continue;
                }

                var slotEnd = DateTime.SpecifyKind(startTime.AddMinutes(_slotDurationMinutes), _dateTimeKind);

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
