using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Domain
{
    public class DaySchedule
    {
        public WorkPeriod WorkPeriod { get; set; }
        public List<TimeSlot> BusySlots { get; set; } = new();
    }
}
