using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Controllers;
using DocplannerAppointmentScheduler.Api.Models;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace DocplannerAppointmentScheduler.Api.Tests
{
    public class SchedulerControllerTests
    {
        private Mock<ISchedulerService> _schedulerServiceMock;
        private Mock<ILogger<SchedulerController>> _loggerMock;
        private Mock<IMapper> _mapperMock;
        private SchedulerController _schedullerController;

        [SetUp]
        public void Setup()
        {
            _schedulerServiceMock = new Mock<ISchedulerService>();
            _loggerMock = new Mock<ILogger<SchedulerController>>();
            _mapperMock = new Mock<IMapper>();
            _schedullerController = new SchedulerController(_schedulerServiceMock.Object, _loggerMock.Object, _mapperMock.Object); 
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithAvailableSlots_WhenFreeSlotsAvailable()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now); 
            int currentYear = DateTime.Now.Year;

            int numberOfAvailableSlots = 2; 
            uint ammountDaySchedules = 1;

            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);
            

            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(currentWeek, currentYear))
                .ReturnsAsync(weeklyAvailability);
            
            //Act
            var result = await _schedullerController.GetAvailableSlots(currentWeek, currentYear);
            
            //Assert

            //Check that the response is an OK
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            //Check that the content of the response is a WeeklyAvailability DTO
            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);

            //Check that the content of the response contains available slots
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count), Is.EqualTo(numberOfAvailableSlots));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithEmptySlots_WhenNoFreeSlotsAvailable()
        {
            // Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;

            int numberOfAvailableSlots = 0;
            uint ammountDaySchedules = 7;

            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);

            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(currentWeek, currentYear))
                .ReturnsAsync(weeklyAvailability);

            // Act
            var result = await _schedullerController.GetAvailableSlots(currentWeek, currentYear);

            // Assert

            
            //Check that the response is an OK
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            //Check that the content of the response is a WeeklyAvailability DTO
            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);

            //Check that the content of the response contains no available slots
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count), Is.EqualTo(numberOfAvailableSlots));
        }

        private WeeklyAvailabilityDTO CreateWeeklyAvailability(int numberOfAvailableSlots, uint ammountDaySchedules)
        {
            List<FreeSlotDTO> availableSlots = CreateAvailableSlots(numberOfAvailableSlots);
            List<DayScheduleDTO> daySchedules = CreateDaySchedules(ammountDaySchedules, availableSlots);
            FacilityDTO facility = CreateFacility("TestFacility", "TestAdress 123");

            return new WeeklyAvailabilityDTO
            {
                Facility = facility,
                DaySchedules = daySchedules,
            };
        }

        private static FacilityDTO CreateFacility(string name, string address)
        {
            return new FacilityDTO
            {
                FacilityId = Guid.NewGuid(),
                Name = name,
                Address = address
            };
        }

        private static List<DayScheduleDTO> CreateDaySchedules(uint ammountDaySchedules, List<FreeSlotDTO> availableSlots)
        {
            var daySchedules = new List<DayScheduleDTO>();

            const int DAYS_IN_A_WEEK = 7;
            if (ammountDaySchedules > DAYS_IN_A_WEEK) ammountDaySchedules = DAYS_IN_A_WEEK;
            for (int i = 0; i < ammountDaySchedules; i++)
            {
                var dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + i);
                var daySchedule = new DayScheduleDTO
                {
                    Day = dayOfWeek.ToString(),
                    AvailableSlots = availableSlots
                };
                daySchedules.Add(daySchedule);
            }

            return daySchedules;
        }

        private static List<FreeSlotDTO> CreateAvailableSlots(int numberOfAvailableSlots)
        {
            int slotDurationMinutes = 10;
            var availableSlots = new List<FreeSlotDTO>();
            for (int i = 0; i < numberOfAvailableSlots; i++)
            {
                availableSlots.Add(new FreeSlotDTO
                {
                    Start = DateTime.Now.AddMinutes(i * slotDurationMinutes + slotDurationMinutes),
                    End = DateTime.Now.AddMinutes(i * slotDurationMinutes + 2 * slotDurationMinutes)
                });
            }
            return availableSlots;
        }
    }
}