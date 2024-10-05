using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Core.DTOs
{
    public class PatientDTO
    {
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
