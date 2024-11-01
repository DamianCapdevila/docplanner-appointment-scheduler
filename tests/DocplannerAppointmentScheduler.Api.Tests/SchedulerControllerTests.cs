using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.Results;
using DocplannerAppointmentScheduler.Api.Controllers;
using DocplannerAppointmentScheduler.Api.Models;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;


namespace DocplannerAppointmentScheduler.Api.Tests
{
    public class SchedulerControllerTests
    {
        private Mock<ISchedulerService> _schedulerServiceMock;
        private Mock<ILogger<SchedulerController>> _loggerMock;
        private SchedulerController _schedulerController;

        [SetUp]
        public void Setup()
        {
            _schedulerServiceMock = new Mock<ISchedulerService>();
            _loggerMock = new Mock<ILogger<SchedulerController>>();
            _schedulerController = new SchedulerController(_schedulerServiceMock.Object, _loggerMock.Object);

        }

        #region GET availableSlots ENDPOINT TESTS
        
        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithAvailableSlots_WhenFreeSlotsAvailable()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            var fakeDataGenerator = new FakeDataGenerator();
            var weeklyAvailability = fakeDataGenerator.GenerateFakeWeeklyAvailabilityDTO(slotDurationMinutes: 10, ammountFreeSlotsPerDay: 2);

            // Mock the GetAvailableSlotsAsync method to return the HttpResponseMessage
            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(await Task.FromResult(Result<WeeklyAvailabilityDTO>.Success(weeklyAvailability)));

            //Act
            var result = await _schedulerController.GetAvailableSlots(request);

            //Assert

            //Check that the response is an OK
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            //Check that the content of the response is a WeeklyAvailability DTO
            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);

            //Check that the content of the response contains available slots
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count),
                Is.EqualTo(weeklyAvailability.DaySchedules.Sum(ds => ds.AvailableSlots.Count)));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithEmptySlots_WhenNoFreeSlotsAvailable()
        {
            // Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            var fakeDataGenerator = new FakeDataGenerator();
            var weeklyAvailability = fakeDataGenerator.GenerateFakeWeeklyAvailabilityDTO(slotDurationMinutes: 10, ammountFreeSlotsPerDay: 0);
            

            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(await Task.FromResult(Result<WeeklyAvailabilityDTO>.Success(weeklyAvailability)));

            //Act
            var result = await _schedulerController.GetAvailableSlots(request);

            //Assert

            //Check that the response is an OK
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            //Check that the content of the response is a WeeklyAvailability DTO
            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);

            //Check that the content of the response contains available slots
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count), Is.EqualTo(weeklyAvailability.DaySchedules.Sum(ds => ds.AvailableSlots.Count)));
        }

        #endregion


        #region POST scheduleAppointment ENDPOINT TESTS
        
        [Test]
        public async Task ScheduleAppointment_ShouldReturnCreated_WithValidRequest_WhenExternalServiceReturnsSuccess()
        {
            // Arrange
            var request = new AppointmentRequest
            {
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(1),
                FacilityId = Guid.NewGuid(),
                Comment = "Hello Docplanner!",
                Patient = new Patient
                {
                    Name = "Damian",
                    SecondName = "Capdevila",
                    Email = "damian.capdevila@i-want-to-work-at-docplanner.com",
                    Phone = "+341234567890"
                }
            };
            
            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>())).
                                 ReturnsAsync(await Task.FromResult(Result<bool>.Success(true)));
            // Act
            
            var result = await _schedulerController.ScheduleAppointment(request);

            // Assert
            var createdResult = result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.That(createdResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        [Test]
        public async Task ScheduleAppointment_ShouldReturnServiceUnavailable_WhenSchedulerService_DoesNotReturnSuccess()
        {
            // Arrange
            var request = new AppointmentRequest
            {
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(1),
                FacilityId = Guid.NewGuid(),
                Comment = "Hello Docplanner!",
                Patient = new Patient
                {
                    Name = "Damian",
                    SecondName = "Capdevila",
                    Email = "damian.capdevila@i-want-to-work-at-docplanner.com",
                    Phone = "+341234567890"
                }
            };
            
            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>()))
                            .ReturnsAsync(await Task.FromResult(Result<bool>.Failure(new Error("Error","Error code"))));

            // Act
            var result = await _schedulerController.ScheduleAppointment(request);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);

            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.ServiceUnavailable));
        }
        #endregion
    }
}