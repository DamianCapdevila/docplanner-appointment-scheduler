namespace DocplannerAppointmentScheduler.Domain
{
    public class WeeklyAvailability
    {
        private IFacilityOccupancy _facilityOccupancy;
        private DateTimeKind _dateTimeKind;
        private List<DaySchedule> _daySchedules;
        private Facility _facility;
        private int _slotDurationMinutes;

        public WeeklyAvailability(IFacilityOccupancy facilityOccupancy)
        {
            _facilityOccupancy = facilityOccupancy;
            _slotDurationMinutes = facilityOccupancy.SlotDurationMinutes;
            _dateTimeKind = DateTimeKind.Unspecified;
            _facility = facilityOccupancy.Facility;
            _daySchedules = new List<DaySchedule>();
        }

        public Facility Facility => _facility;
        public IReadOnlyList<DaySchedule> DaySchedules => _daySchedules;
        public WeeklyAvailability GetAvailability(DateTime startOfTheWeek) => CalculateAvailability(startOfTheWeek);
        

        private WeeklyAvailability CalculateAvailability(DateTime startOfTheWeek)
        {
            _daySchedules.Clear();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                DayOccupancy? dayOccupancy = day switch
                {
                    DayOfWeek.Monday => _facilityOccupancy.Monday,
                    DayOfWeek.Tuesday => _facilityOccupancy.Tuesday,
                    DayOfWeek.Wednesday => _facilityOccupancy.Wednesday,
                    DayOfWeek.Thursday => _facilityOccupancy.Thursday,
                    DayOfWeek.Friday => _facilityOccupancy.Friday,
                    DayOfWeek.Saturday => _facilityOccupancy.Saturday,
                    DayOfWeek.Sunday => _facilityOccupancy.Sunday,
                    _ => null
                };
                var daysOffset = (int)day - (int)DayOfWeek.Monday;
                if (daysOffset < 0) daysOffset += 7; //This happens on Sunday.

                var dateOfTheDay = startOfTheWeek.AddDays(daysOffset);
                _daySchedules.Add(DailyAvailability(day, dayOccupancy, dateOfTheDay));   
            }
            return this;
        }

        private DaySchedule DailyAvailability(DayOfWeek day, DayOccupancy? dayOccupancy, DateTime dateOfTheDay)
        {
            if (dayOccupancy == null || dayOccupancy.WorkPeriod == null)
            {
                return NoMedicalAttentionProvided(day);
            }

            var workPeriod = dayOccupancy.WorkPeriod;
            var availableSlots = new List<FreeSlot>();

            var startTime = DateTime.SpecifyKind(dateOfTheDay.AddHours(workPeriod.StartHour), _dateTimeKind);
            var endTime = DateTime.SpecifyKind(dateOfTheDay.AddHours(workPeriod.EndHour), _dateTimeKind);
            var lunchStart = DateTime.SpecifyKind(dateOfTheDay.AddHours(workPeriod.LunchStartHour), _dateTimeKind);
            var lunchEnd = DateTime.SpecifyKind(dateOfTheDay.AddHours(workPeriod.LunchEndHour), _dateTimeKind);

            while (startTime < endTime)
            {
                if (IsLunchTime(startTime, lunchStart, lunchEnd))
                {
                    startTime = lunchEnd;
                    continue;
                }

                var slotEnd = DateTime.SpecifyKind(startTime.AddMinutes(_slotDurationMinutes), _dateTimeKind);

                if (!IsSlotBusy(startTime, slotEnd, dayOccupancy.BusySlots))
                {
                    AddFreeSlot(availableSlots, startTime, slotEnd);
                }

                startTime = slotEnd;
            }

            return new DaySchedule
            {
                Day = day.ToString(),
                AvailableSlots = availableSlots
            };
        }

        private static void AddFreeSlot(List<FreeSlot> availableSlots, DateTime startTime, DateTime slotEnd)
        {
            availableSlots.Add(new FreeSlot
            {
                Start = startTime,
                End = slotEnd
            });
        }

        private bool IsSlotBusy(DateTime slotStart, DateTime slotEnd, List<BusySlot> busySlots)
        {
            return busySlots.Any(busySlot =>
                (slotStart >= busySlot.Start && slotStart < busySlot.End) ||
                (slotEnd > busySlot.Start && slotEnd <= busySlot.End));
        }

        private static DaySchedule NoMedicalAttentionProvided(DayOfWeek day)
        {
            return new DaySchedule
            {
                Day = day.ToString(),
                AvailableSlots = new List<FreeSlot>()
            };
        }

        private bool IsLunchTime(DateTime currentTime, DateTime lunchStart, DateTime lunchEnd)
        {
            return currentTime >= lunchStart && currentTime < lunchEnd;
        }
    }

    public class DaySchedule
    {
        public required string Day { get; set; }
        public required List<FreeSlot> AvailableSlots { get; set; }
    }

    public class FreeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
