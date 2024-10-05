using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Controllers;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Api.Models;
using Microsoft.AspNetCore.Http;

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
        public async Task GetAvailableSlots_ShouldReturnOk_WithAvailableSlots()
        {
            //Arrange
            var date = DateTime.Now;
            var availableSlots = new List<TimeSlotDTO>
            { 
                new TimeSlotDTO{Start = date.AddMinutes(10), End = date.AddMinutes(20)},
                new TimeSlotDTO{Start = date.AddMinutes(20), End = date.AddMinutes(30)}
            };
            _schedulerServiceMock.Setup(s=> s.GetAvailableSlots(It.IsAny<DateTime>()))
                .ReturnsAsync(availableSlots);
            //Act
            var result = await _schedullerController.GetAvailableSlots(date);
            
            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var response = okResult.Value as AvailableSlotsResponse;
            Assert.IsNotNull(response);

            Assert.That(response.Slots.Count, Is.EqualTo(availableSlots.Count));

        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnInternalServerError_OnException()
        {
            //Arrange
            var date = DateTime.Now;
            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception());
            //Act
            var result = await _schedullerController.GetAvailableSlots(date);

            //Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);

            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
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