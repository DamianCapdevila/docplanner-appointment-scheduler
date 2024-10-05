using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Domain
{
    public class Appointment
    {
        public Slot Slot { get; set; }
        public string Comments { get; set; }
        public Patient Patient { get; set; }
        
    }
}
