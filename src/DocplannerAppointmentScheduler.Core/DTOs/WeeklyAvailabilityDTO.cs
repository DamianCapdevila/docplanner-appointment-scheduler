using Newtonsoft.Json;

namespace DocplannerAppointmentScheduler.Core.DTOs
{
    /// <summary>
    /// Based on the calculations done in the Domain layer, this class will be used to talk to our API and present the available time slots.
    /// </summary>
    public class WeeklyAvailabilityDTO
    {
        [JsonProperty("facility")]
        public FacilityDTO Facility { get; set; }

        [JsonProperty("daySchedules")]
        public List<DayScheduleDTO> DaySchedules { get; set; }
    }

    public class DayScheduleDTO
    {
        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("availableSlots")]
        public List<FreeSlotDTO> AvailableSlots { get; set; }
    }

    public class FreeSlotDTO
    {
        [JsonProperty("start")]
        [JsonConverter(typeof(DateTimeWithoutTimezoneConverter))]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        [JsonConverter(typeof(DateTimeWithoutTimezoneConverter))]
        public DateTime End { get; set; }
    }
}
