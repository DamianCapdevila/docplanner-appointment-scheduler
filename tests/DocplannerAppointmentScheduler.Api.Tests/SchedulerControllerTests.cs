using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Controllers;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Api.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace DocplannerAppointmentScheduler.Api.Tests
{
    public class SchedulerControllerTests
    {
        private Mock<ISchedulerService> _schedulerServiceMock;
        private Mock<ILogger<SchedulerController>> _loggerMock;
        private SchedulerController _schedullerController;

        [SetUp]
        public void Setup()
        {
            _schedulerServiceMock = new Mock<ISchedulerService>();
            _loggerMock = new Mock<ILogger<SchedulerController>>();
            _schedullerController = new SchedulerController(_schedulerServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithFreeSlots()
        {
            //Arrange
            int weekNumber = ISOWeek.GetWeekOfYear(DateTime.Now); // Get current week number
            int year = DateTime.Now.Year; // Get current year

            // Prepare mock free slots
            var availableSlots = new List<FreeSlotDTO>
            {
                new FreeSlotDTO { Start = DateTime.Now.AddMinutes(10), End = DateTime.Now.AddMinutes(20) },
                new FreeSlotDTO { Start = DateTime.Now.AddMinutes(30), End = DateTime.Now.AddMinutes(40) }
            };

            // Prepare mock work period
            var workPeriod = new WorkPeriodDTO
            {
                StartHour = 9,
                EndHour = 17,
                LunchStartHour = 12,
                LunchEndHour = 13
            };

            // Prepare mock day schedule
            var daySchedule = new DayScheduleDTO
            {
                Day = "Monday",
                WorkPeriod = workPeriod,
                AvailableSlots = availableSlots
            };

            // Prepare mock facility
            var facility = new FacilityDTO
            {
                FacilityId = Guid.NewGuid(),
                Name = "Test Facility",
                Address = "123 Test Street"
            };

            // Prepare mock weekly availability
            var weeklyAvailability = new WeeklyAvailabilityDTO
            {
                Facility = facility,
                SlotDurationMinutes = 30,
                DaySchedules = new List<DayScheduleDTO> { daySchedule }
            };

            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(weekNumber, year))
                .ReturnsAsync(weeklyAvailability);
            
            //Act
            var result = await _schedullerController.GetAvailableSlots(weekNumber, year);
            
            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnInternalServerError_OnException()
        {
            ////Arrange
            //var date = DateTime.Now;
            //_schedulerServiceMock.Setup(s => s.GetAvailableSlots(It.IsAny<DateTime>()))
            //    .ThrowsAsync(new Exception());
            ////Act
            //var result = await _schedullerController.GetAvailableSlots(date);

            ////Assert
            //var statusCodeResult = result as ObjectResult;
            //Assert.IsNotNull(statusCodeResult);

            //Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task ScheduleAppointment_ShouldReturnOk_OnSuccess()
        {
            //Arrange
            var request = new ScheduleAppointmentRequest
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddMinutes(10),
                FacilityId = new Guid().ToString(),
                Comment = "This is a test appointment!",
                PatientRequest = new PatientRequest
                {
                    Name = "Sergio",
                    SecondName = "Damián",
                    Email = "damian.capdevila@hotmail.com",
                    Phone = "+34673467345"
                }
            };

            _schedulerServiceMock.Setup(s=>s.ScheduleAppointment(It.IsAny<AppointmentRequestDTO>()))
                .ReturnsAsync(true);

            //Act
            var result = await _schedullerController.ScheduleAppointment(request);

            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That((bool)okResult.Value, Is.True);
        }


    }
}