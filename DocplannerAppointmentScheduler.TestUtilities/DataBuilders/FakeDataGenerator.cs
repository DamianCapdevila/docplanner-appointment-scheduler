using Bogus;
using DocplannerAppointmentScheduler.Core.DTOs;
using System.Net;
using DocplannerAppointmentScheduler.TestUtilities.Enums;
using System.Text;
using DocplannerAppointmentScheduler.Domain;
using Newtonsoft.Json;

namespace DocplannerAppointmentScheduler.TestUtilities.DataBuilders
{
    public class FakeDataGenerator
    {
        public WeeklyAvailabilityDTO GenerateFakeWeeklyAvailabilityDTO(int slotDurationMinutes, int ammountFreeSlotsPerDay)
        {

            var facilityFaker = new Faker<FacilityDTO>()
                .RuleFor(f => f.FacilityId, f => Guid.NewGuid())
                .RuleFor(f => f.Name, f => f.Company.CompanyName())
                .RuleFor(f => f.Address, f => f.Address.FullAddress());


            var freeSlotFaker = new Faker<FreeSlotDTO>()
                .RuleFor(f => f.Start, f => f.Date.Between(DateTime.Now, DateTime.Now.AddDays(7)))
                .RuleFor(f => f.End, (f, fs) => fs.Start.AddMinutes(slotDurationMinutes));


            var dayScheduleFaker = new Faker<DayScheduleDTO>()
                .RuleFor(d => d.Day, f => f.Date.Weekday().ToString())
                .RuleFor(d => d.AvailableSlots, f => freeSlotFaker.Generate(ammountFreeSlotsPerDay));


            var weeklyAvailabilityFaker = new Faker<WeeklyAvailabilityDTO>()
                .RuleFor(w => w.Facility, f => facilityFaker.Generate())
                .RuleFor(w => w.DaySchedules, f => dayScheduleFaker.Generate(7));

            return weeklyAvailabilityFaker.Generate();
        }

        public FacilityOccupancyDTO GenerateFakeFacilityOccupancyDTO(int slotDurationMinutes, int busySlotsPerDay, DateTime startOfTheWeek)
        {
            var facilityFaker = new Faker<FacilityDTO>()
                .RuleFor(f => f.FacilityId, f => Guid.NewGuid())
                .RuleFor(f => f.Name, f => f.Company.CompanyName())
                .RuleFor(f => f.Address, f => f.Address.FullAddress());

            var busySlotFaker = new Faker<BusySlotDTO>()
                .RuleFor(b => b.Start, f => f.Date.Between(startOfTheWeek, startOfTheWeek.AddDays(7)))
                .RuleFor(b => b.End, (f, b) => b.Start.AddMinutes(slotDurationMinutes));

            var workPeriodFaker = new Faker<WorkPeriodDTO>()
                .RuleFor(w => w.StartHour, f => f.Random.Int(8, 10))  
                .RuleFor(w => w.EndHour, f => f.Random.Int(16, 18))    
                .RuleFor(w => w.LunchStartHour, f => f.Random.Int(12, 13))  
                .RuleFor(w => w.LunchEndHour, (f, w) => w.LunchStartHour + 1);  

            var dayOccupancyFaker = new Faker<DayOccupancyDTO>()
                .RuleFor(d => d.WorkPeriod, f => workPeriodFaker.Generate())
                .RuleFor(d => d.BusySlots, f => busySlotFaker.Generate(busySlotsPerDay));

            var facilityOccupancyFaker = new Faker<FacilityOccupancyDTO>()
                .RuleFor(o => o.Facility, f => facilityFaker.Generate())
                .RuleFor(o => o.SlotDurationMinutes, slotDurationMinutes)
                .RuleFor(o => o.Monday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Tuesday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Wednesday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Thursday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Friday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Saturday, f => dayOccupancyFaker.Generate())
                .RuleFor(o => o.Sunday, f => dayOccupancyFaker.Generate());

            return facilityOccupancyFaker.Generate();
        }

        public FacilityOccupancy GenerateFakeFacilityOccupancy(int slotDurationMinutes, int busySlotsPerDay, DateTime startOfTheWeek)
        {
            var workPeriod = new WorkPeriod
            {
                StartHour = 8,
                EndHour = 17,
                LunchStartHour = 12,
                LunchEndHour = 13
            };

            int workDayLengthMinutes = ((workPeriod.EndHour - workPeriod.StartHour) - (workPeriod.LunchEndHour - workPeriod.LunchStartHour)) * 60;
            int totalSlotsPerDay = workDayLengthMinutes / slotDurationMinutes;

            if (busySlotsPerDay > totalSlotsPerDay)
            {
                busySlotsPerDay = totalSlotsPerDay;
            }

            var facilityFaker = new Faker<Facility>()
                .RuleFor(f => f.FacilityId, f => Guid.NewGuid())
                .RuleFor(f => f.Name, f => f.Company.CompanyName())
                .RuleFor(f => f.Address, f => f.Address.FullAddress());

            // Create busy slots for each day of the week
            var busySlotsByDay = new Dictionary<DayOfWeek, List<BusySlot>>();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var busySlots = GenerateBusySlots(busySlotsPerDay, slotDurationMinutes, startOfTheWeek, workPeriod, day);
                busySlotsByDay[day] = busySlots;
            }

            // Create DayOccupancy for each day using the correct busy slots
            var facilityOccupancyFaker = new Faker<FacilityOccupancy>()
                .RuleFor(o => o.Facility, f => facilityFaker.Generate())
                .RuleFor(o => o.SlotDurationMinutes, slotDurationMinutes)
                .RuleFor(o => o.Monday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Monday]
                })
                .RuleFor(o => o.Tuesday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Tuesday]
                })
                .RuleFor(o => o.Wednesday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Wednesday]
                })
                .RuleFor(o => o.Thursday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Thursday]
                })
                .RuleFor(o => o.Friday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Friday]
                })
                .RuleFor(o => o.Saturday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Saturday]
                })
                .RuleFor(o => o.Sunday, f => new DayOccupancy
                {
                    WorkPeriod = workPeriod,
                    BusySlots = busySlotsByDay[DayOfWeek.Sunday]
                });

            return facilityOccupancyFaker.Generate();
        }


        private List<BusySlot> GenerateBusySlots(int busySlotsPerDay, int slotDurationMinutes, DateTime startOfTheWeek, WorkPeriod workPeriod, DayOfWeek dayOfWeek)
        {
            var busySlots = new List<BusySlot>();

            var daysOffset = (int)dayOfWeek - (int)DayOfWeek.Monday;
            if (daysOffset < 0) daysOffset += 7; 

            var dateOfTheDay = startOfTheWeek.AddDays(daysOffset);

            
            DateTime workStart = dateOfTheDay.AddHours(workPeriod.StartHour);
            DateTime workEnd = dateOfTheDay.AddHours(workPeriod.EndHour);
            DateTime lunchStart = dateOfTheDay.AddHours(workPeriod.LunchStartHour);
            DateTime lunchEnd = dateOfTheDay.AddHours(workPeriod.LunchEndHour);

            DateTime startSlot = workStart;

            for (int i = 0; i < busySlotsPerDay; i++)
            {
                if (IsLunchTime(lunchStart, lunchEnd, startSlot))
                {
                    startSlot = lunchEnd;
                }

                DateTime endSlot = startSlot.AddMinutes(slotDurationMinutes);
                if (endSlot > workEnd)
                {
                    break;
                }

                var busySlot = new BusySlot
                {
                    Start = startSlot,
                    End = endSlot
                };
                busySlots.Add(busySlot);

                startSlot = endSlot;
            }
            return busySlots;
        }

        private static bool IsLunchTime(DateTime lunchStart, DateTime lunchEnd, DateTime startSlot)
        {
            return startSlot >= lunchStart && startSlot < lunchEnd;
        }

        public AppointmentRequestDTO GenerateFakeAppointmentRequestDTO()
        {
            var appointmentFaker = new Faker<AppointmentRequestDTO>()
                .RuleFor(a => a.Start, f => f.Date.Between(DateTime.Now, DateTime.Now.AddDays(7)))
                .RuleFor(a => a.End, (f, a) => a.Start.AddMinutes(10))
                .RuleFor(a => a.FacilityId, f => Guid.NewGuid())
                .RuleFor(a => a.Comment, f => f.Lorem.Sentence())
                .RuleFor(a => a.Patient, f => GenerateFakePatientDTO());

            return appointmentFaker.Generate();
        }

        public PatientDTO GenerateFakePatientDTO()
        {
            var patientFaker = new Faker<PatientDTO>()
                .RuleFor(p => p.Name, f => f.Name.FirstName())
                .RuleFor(p => p.SecondName, f => f.Name.LastName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber());

            return patientFaker.Generate();
        }

        public HttpResponseMessage GenerateFakeHttpResponse(object? content = null, StatusCodeRange range = StatusCodeRange.All)
        {
            var allStatusCodes = Enum.GetValues(typeof(HttpStatusCode)).Cast<HttpStatusCode>().ToList();
            var filteredStatusCodes = new List<HttpStatusCode>();

            // If AllButSuccess is specified, we can exclude success codes directly
            if (range.HasFlag(StatusCodeRange.AllButSuccess))
            {
                filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status < 200 || (int)status >= 300));
            }
            else
            {
                // Filter status codes based on specified ranges
                if (range.HasFlag(StatusCodeRange.Informational))
                {
                    filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status >= 100 && (int)status < 200));
                }

                if (range.HasFlag(StatusCodeRange.Success))
                {
                    filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status >= 200 && (int)status < 300));
                }

                if (range.HasFlag(StatusCodeRange.Redirection))
                {
                    filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status >= 300 && (int)status < 400));
                }

                if (range.HasFlag(StatusCodeRange.ClientError))
                {
                    filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status >= 400 && (int)status < 500));
                }

                if (range.HasFlag(StatusCodeRange.ServerError))
                {
                    filteredStatusCodes.AddRange(allStatusCodes.Where(status => (int)status >= 500));
                }
            }

            if (!filteredStatusCodes.Any())
            {
                throw new InvalidOperationException("No status codes available for the specified range.");
            }

            var responseFaker = new Faker<HttpResponseMessage>()
                .RuleFor(r => r.StatusCode, f => f.PickRandom(filteredStatusCodes));

            if (content != null)
            {
                var serializedContent = JsonConvert.SerializeObject(content);
                responseFaker.RuleFor(r => r.Content, _ => new StringContent(serializedContent, Encoding.UTF8, "application/json"));
            }
            else
            {
                responseFaker.RuleFor(r => r.Content, f => new StringContent(f.Lorem.Sentence()));
            }

            return responseFaker.Generate();
        }
    }
}
