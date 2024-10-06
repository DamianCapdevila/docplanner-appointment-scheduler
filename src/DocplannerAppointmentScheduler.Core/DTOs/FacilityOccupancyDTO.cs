using Newtonsoft.Json;

namespace DocplannerAppointmentScheduler.Core.DTOs
{
    /// <summary>
    /// Holds the data deserialized from the external API response. Will be used to talk to the Domain, where we will do the business logic calculations.
    /// </summary>
    public class FacilityOccupancyDTO
    {
        [JsonProperty("Facility")]
        public FacilityDTO Facility { get; set; }

        [JsonProperty("SlotDurationMinutes")]
        public int SlotDurationMinutes { get; set; }

        [JsonProperty("Monday")]
        public DayOccupancyDTO Monday { get; set; }

        [JsonProperty("Tuesday")]
        public DayOccupancyDTO Tuesday { get; set; }

        [JsonProperty("Wednesday")]
        public DayOccupancyDTO Wednesday { get; set; }

        [JsonProperty("Thursday")]
        public DayOccupancyDTO Thursday { get; set; }

        [JsonProperty("Friday")]
        public DayOccupancyDTO Friday { get; set; }
    }

    public class FacilityDTO
    {
        [JsonProperty("FacilityId")]
        public Guid FacilityId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }
    }

    public class DayOccupancyDTO
    {
        [JsonProperty("WorkPeriod")]
        public WorkPeriodDTO WorkPeriod { get; set; }

        [JsonProperty("BusySlots")]
        public List<BusySlotDTO> BusySlots { get; set; }
    }

    public class WorkPeriodDTO
    {
        [JsonProperty("StartHour")]
        public int StartHour { get; set; }

        [JsonProperty("EndHour")]
        public int EndHour { get; set; }

        [JsonProperty("LunchStartHour")]
        public int LunchStartHour { get; set; }

        [JsonProperty("LunchEndHour")]
        public int LunchEndHour { get; set; }
    }

    public class BusySlotDTO
    {
        [JsonProperty("Start")]
        public DateTime Start { get; set; }

        [JsonProperty("End")]
        public DateTime End { get; set; }
    }
}
