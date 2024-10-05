using DocplannerAppointmentScheduler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Core.DTOs
{
    public class AppointmentRequestDTO
    {
        public TimeSlotDTO Slot { get; set; }

        public string FacilityId { get; set; }

        public string Comment { get; set; }
        public PatientDTO Patient { get; set; }
    }
}
