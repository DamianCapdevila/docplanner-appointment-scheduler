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
    }
}
