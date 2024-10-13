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
        public void CalculateAvailability_ShouldReturnAllSlotsAvailable_WhenNoBusySlots()
        {
            // Arrange
            int slotDurationMinutes = 10;
            int busySlotsPerDay = 0;

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            DateTime mondayOfThisWeek = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);

            int workDayLengthMinutes = 8 * 60; // We assume a workday of 8 hours for this test.
            int totalSlotsPerDay = workDayLengthMinutes / slotDurationMinutes;

            int expectedAvailableSlotsPerWeek = 7 * (totalSlotsPerDay - busySlotsPerDay);

            var fakeFacilityOccupancy = _dataGenerator.GenerateFakeFacilityOccupancy(slotDurationMinutes, busySlotsPerDay, mondayOfThisWeek);

            // Act
            var weeklyAvailability = new WeeklyAvailability(fakeFacilityOccupancy).GetAvailability(mondayOfThisWeek);

            // Assert
            
            int actualAvailableSlots = weeklyAvailability.DaySchedules.Sum(daySchedule => daySchedule.AvailableSlots.Count);
            Assert.That(actualAvailableSlots, Is.EqualTo(expectedAvailableSlotsPerWeek), "Total available slots do not match the expected count.");
        }

        [Test]
        public void CalculateAvailability_ShouldReturnCorrectAmmountOfFreeSlots_WhenSomeSlotsBusy()
        {
            // Arrange
            int slotDurationMinutes = 10;
            int busySlotsPerDay = 10;

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            DateTime mondayOfThisWeek = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);

            int workDayLengthMinutes = 8 * 60; // We assume a workday of 8 hours for this test.
            int totalSlotsPerDay = workDayLengthMinutes / slotDurationMinutes;

            int expectedAvailableSlotsPerWeek = 7 * (totalSlotsPerDay - busySlotsPerDay);


            var fakeFacilityOccupancy = _dataGenerator.GenerateFakeFacilityOccupancy(slotDurationMinutes, busySlotsPerDay, mondayOfThisWeek);

            // Act
            var weeklyAvailability = new WeeklyAvailability(fakeFacilityOccupancy).GetAvailability(mondayOfThisWeek);

            // Assert
            int actualAvailableSlots = weeklyAvailability.DaySchedules.Sum(daySchedule => daySchedule.AvailableSlots.Count);
            Assert.That(actualAvailableSlots, Is.EqualTo(expectedAvailableSlotsPerWeek), "Total available slots do not match the expected count.");
        }

        [Test]
        public void CalculateAvailability_ShouldReturnNoSlotsAvailable_WhenAllSlotsBusy()
        {
            // Arrange
            int slotDurationMinutes = 10;


            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            DateTime mondayOfThisWeek = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);

            int workDayLengthMinutes = 8 * 60; // We assume a workday of 8 hours for this test.
            int totalSlotsPerDay = workDayLengthMinutes / slotDurationMinutes;

            int expectedAvailableSlotsPerWeek = 0;


            var fakeFacilityOccupancy = _dataGenerator.GenerateFakeFacilityOccupancy(slotDurationMinutes, busySlotsPerDay: totalSlotsPerDay, mondayOfThisWeek);

            // Act
            var weeklyAvailability = new WeeklyAvailability(fakeFacilityOccupancy).GetAvailability(mondayOfThisWeek);

            // Assert
            int actualAvailableSlots = weeklyAvailability.DaySchedules.Sum(daySchedule => daySchedule.AvailableSlots.Count);
            Assert.That(actualAvailableSlots, Is.EqualTo(expectedAvailableSlotsPerWeek), "Total available slots do not match the expected count.");
        }
    }
}