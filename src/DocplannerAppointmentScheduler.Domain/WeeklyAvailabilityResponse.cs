using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Domain
{
    public class WeeklyAvailabilityResponse
    {
        public Facility Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
        public DaySchedule Monday { get; set; }
        public DaySchedule Tuesday { get; set; }
        public DaySchedule Wednesday { get; set; }
        public DaySchedule Thursday { get; set; }
        public DaySchedule Friday { get; set; }
    }

}
