using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Domain
{
    public class WorkPeriod
    {
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public int LunchStartHour { get; set; }
        public int LunchEndHour { get; set; }
    }
}
