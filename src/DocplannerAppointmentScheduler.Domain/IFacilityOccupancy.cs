namespace DocplannerAppointmentScheduler.Domain
{
    public interface IFacilityOccupancy
    {
        Facility Facility { get; set; }
        DayOccupancy? Friday { get; set; }
        DayOccupancy? Monday { get; set; }
        DayOccupancy? Saturday { get; set; }
        int SlotDurationMinutes { get; set; }
        DayOccupancy? Sunday { get; set; }
        DayOccupancy? Thursday { get; set; }
        DayOccupancy? Tuesday { get; set; }
        DayOccupancy? Wednesday { get; set; }
    }
}