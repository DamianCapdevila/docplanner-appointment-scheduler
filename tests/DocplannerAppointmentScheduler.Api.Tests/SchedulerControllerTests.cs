using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Controllers;
using DocplannerAppointmentScheduler.Api.Models;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using AutoMapper;
using System.Net;
using Bogus;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;
using DocplannerAppointmentScheduler.TestUtilities.Enums;
using Newtonsoft.Json;
using System.Text;


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
            
            var fakeResponse = fakeDataGenerator.GenerateFakeHttpResponse(weeklyAvailability, range: StatusCodeRange.Success);

            // Mock the GetAvailableSlotsAsync method to return the HttpResponseMessage
            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(fakeResponse);

            //Act
            var result = await _schedullerController.GetAvailableSlots(request);

            //Assert

            //Check that the response is an OK
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            //Check that the content of the response is a WeeklyAvailability DTO
            var response = okResult.Value as WeeklyAvailabilityDTO;
            Assert.IsNotNull(response);

            //Check that the content of the response contains available slots
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count), Is.EqualTo(weeklyAvailability.DaySchedules.Sum(ds=>ds.AvailableSlots.Count)));
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
            
            var fakeResponse = fakeDataGenerator.GenerateFakeHttpResponse(weeklyAvailability, range: StatusCodeRange.Success);

            
            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(fakeResponse);

            //Act
            var result = await _schedullerController.GetAvailableSlots(request);

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
                PatientRequest = new PatientRequest
                {
                    Name = "Damian",
                    SecondName = "Capdevila",
                    Email = "damian.capdevila@i-want-to-work-at-docplanner.com",
                    Phone = "+341234567890"
                }
            };


            var appointmentRequestDto = new AppointmentRequestDTO();
            _mapperMock.Setup(m => m.Map<AppointmentRequestDTO>(request)).Returns(appointmentRequestDto);


            var fakeDataGenerator = new FakeDataGenerator();
            var successResponseMessage = fakeDataGenerator.GenerateFakeHttpResponse(range: StatusCodeRange.Success);


            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>())).ReturnsAsync(successResponseMessage);

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

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
                PatientRequest = new PatientRequest
                {
                    Name = "Damian",
                    SecondName = "Capdevila",
                    Email = "damian.capdevila@i-want-to-work-at-docplanner.com",
                    Phone = "+341234567890"
                }
            };

            var appointmentRequestDto = new AppointmentRequestDTO();
            _mapperMock.Setup(m => m.Map<AppointmentRequestDTO>(request)).Returns(appointmentRequestDto);


            var fakeDataGenerator = new FakeDataGenerator();
            var unsuccessfullResponseMessage = fakeDataGenerator.GenerateFakeHttpResponse(range: StatusCodeRange.AllButSuccess);


            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>()))
                            .ReturnsAsync(unsuccessfullResponseMessage);

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);

            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.ServiceUnavailable));
        }
        #endregion
    }
}