﻿namespace DocplannerAppointmentScheduler.Domain
{
    /// <summary>
    /// Represents the occupancy data for a facility.
    /// </summary>
    public class FacilityOccupancy : IFacilityOccupancy
    {
        public required Facility Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
        public  DayOccupancy? Monday { get; set; }
        public  DayOccupancy? Tuesday { get; set; }
        public  DayOccupancy? Wednesday { get; set; }
        public  DayOccupancy? Thursday { get; set; }
        public  DayOccupancy? Friday { get; set; }
        public  DayOccupancy? Saturday { get; set; }
        public  DayOccupancy? Sunday { get; set; }
    }

    /// <summary>
    /// Represents a facility with its details.
    /// </summary>
    public class Facility
    {
        public Guid FacilityId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
    }

    /// <summary>
    /// Represents the occupancy details for a specific day.
    /// </summary>
    public class DayOccupancy
    {
        public required WorkPeriod WorkPeriod { get; set; }
        public required List<BusySlot> BusySlots { get; set; }
    }

    /// <summary>
    /// Represents the working hours and lunch period.
    /// </summary>
    public class WorkPeriod
    {
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public int LunchStartHour { get; set; }
        public int LunchEndHour { get; set; }
    }

    /// <summary>
    /// Represents a time slot that is busy.
    /// </summary>
    public class BusySlot
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
