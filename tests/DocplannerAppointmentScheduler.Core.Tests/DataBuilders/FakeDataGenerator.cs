using Bogus;
using System;
using System.Collections.Generic;
using DocplannerAppointmentScheduler.Core.DTOs;

namespace DocplannerAppointmentScheduler.Core.Tests.DataBuilders
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
    }
}
