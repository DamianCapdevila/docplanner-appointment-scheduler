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
        public WeeklyAvailabilityDTO GenerateFakeWeeklyAvailability(int slotDurationMinutes, int ammountFreeSlotsPerDay)
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

        public AppointmentRequestDTO GenerateFakeAppointmentRequest()
        {
            var appointmentFaker = new Faker<AppointmentRequestDTO>()
                .RuleFor(a => a.Start, f => f.Date.Between(DateTime.Now, DateTime.Now.AddDays(7)))
                .RuleFor(a => a.End, (f, a) => a.Start.AddMinutes(10))
                .RuleFor(a => a.FacilityId, f => Guid.NewGuid())
                .RuleFor(a => a.Comment, f => f.Lorem.Sentence())
                .RuleFor(a => a.Patient, f => GenerateFakePatient());

            return appointmentFaker.Generate();
        }

        public PatientDTO GenerateFakePatient()
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
