using Moq;
using Bogus;
using DocplannerAppointmentScheduler.TestUtilities;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;
using System.Globalization;


namespace DocplannerAppointmentScheduler.Domain.Tests
{
    public class WeeklyAvailabilityTests
    {
        private Mock<IFacilityOccupancy> _facilityOccupancyMock;
        private FakeDataGenerator _dataGenerator;
        private WeeklyAvailability _weeklyAvailability;

        [SetUp]
        public void Setup()
        {
            _dataGenerator = new FakeDataGenerator();
            _facilityOccupancyMock = new Mock<IFacilityOccupancy>();
            _weeklyAvailability = new WeeklyAvailability(_facilityOccupancyMock.Object);
        }

        [Test]
        public void CalculateAvailability_NoBusySlots_ReturnsAllSlotsAvailable()
        {
            // Arrange
            int slotDurationMinutes = 10;
            int busySlotsPerDay = 0;

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            DateTime mondayOfThisWeek = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);


            var fakeFacilityOccupancy = _dataGenerator.GenerateFakeFacilityOccupancy(slotDurationMinutes, busySlotsPerDay, mondayOfThisWeek);
            var weeklyAvailability = new WeeklyAvailability(fakeFacilityOccupancy);

            // Act
            var availability = weeklyAvailability.GetAvailability(mondayOfThisWeek);

            // Calculate expected available slots based on the work period from the fake occupancy data
            int totalExpectedAvailableSlots = CalculateExpectedAvailableSlots(slotDurationMinutes, fakeFacilityOccupancy);

            // Calculate the total available slots from the real availability object
            int totalAvailableSlots = availability.DaySchedules.Sum(daySchedule => daySchedule.AvailableSlots.Count);

            // Assert
            Assert.That(totalAvailableSlots, Is.EqualTo(totalExpectedAvailableSlots), "Total available slots do not match the expected count.");
        }

        private static int CalculateExpectedAvailableSlots(int slotDurationMinutes, FacilityOccupancy facilityOccupancy)
        {
            int totalExpectedAvailableSlots = 0;

            foreach (var dayOccupancy in facilityOccupancy.GetType().GetProperties().Where(p => p.PropertyType == typeof(DayOccupancy)))
            {
                var dayOccupancyData = dayOccupancy.GetValue(facilityOccupancy) as DayOccupancy;

                if (dayOccupancyData != null)
                {
                    // Calculate available slots based on work period
                    int startHour = dayOccupancyData.WorkPeriod.StartHour;
                    int endHour = dayOccupancyData.WorkPeriod.EndHour;
                    int lunchStartHour = dayOccupancyData.WorkPeriod.LunchStartHour;
                    int lunchEndHour = dayOccupancyData.WorkPeriod.LunchEndHour;

                    // Calculate the total available minutes, accounting for lunch breaks
                    int availableMinutes = (endHour - startHour) * 60; // Total minutes available for appointments

                    // Subtract lunch break minutes if lunch falls within the working hours
                    if (startHour < lunchEndHour && endHour > lunchStartHour)
                    {
                        int lunchDuration = Math.Min(endHour, lunchEndHour) - Math.Max(startHour, lunchStartHour);
                        if (lunchDuration > 0)
                        {
                            availableMinutes -= lunchDuration * 60; // Convert hours to minutes
                        }
                    }

                    int availableSlotsForDay = availableMinutes / slotDurationMinutes; // Number of available slots

                    totalExpectedAvailableSlots += availableSlotsForDay;
                }
            }

            return totalExpectedAvailableSlots;
        }
    }
}