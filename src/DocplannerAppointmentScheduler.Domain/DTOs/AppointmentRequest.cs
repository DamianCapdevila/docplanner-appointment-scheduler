using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Domain.DTOs
{
    public class AppointmentRequest
    {
        public TimeSlot Slot { get; set; }

        public string FacilityId { get; set; }

        public string Comment { get; set; }
        public Patient Patient { get; set; }
    }
}

