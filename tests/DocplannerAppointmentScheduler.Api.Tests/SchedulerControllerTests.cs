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
            var weeklyAvailability = fakeDataGenerator.GenerateFakeWeeklyAvailability(slotDurationMinutes: 10, ammountFreeSlotsPerDay: 2);
            
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
            var weeklyAvailability = fakeDataGenerator.GenerateFakeWeeklyAvailability(slotDurationMinutes: 10, ammountFreeSlotsPerDay: 0);
            
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

 
        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithValidationErrorDetails_WhenSelectedWeekIsInThePast()
        {
            // Arrange
            int pastWeek = ISOWeek.GetWeekOfYear(DateTime.Now) - 1;
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = pastWeek, Year = currentYear };

            _schedullerController.ModelState.AddModelError("WeekNumber", "The selected week has already passed. Please choose a future week.");
            // Act
            var result = await _schedullerController.GetAvailableSlots(request);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.IsNotNull(modelState);
            Assert.That(modelState.ContainsKey("WeekNumber"), Is.True);
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithValidationErrorDetails_WhenInvalidWeek()
        {
            // Arrange
            int invalidWeek = 60;  //A year has between 1 and  53 weeks.
            int currentYear = DateTime.Now.Year;
            var invalidRequest = new AvailableSlotsRequest
            {
                WeekNumber = invalidWeek,
                Year = currentYear
            };

            // Act
            _schedullerController.ModelState.AddModelError("WeekNumber", "The week number must be between 1 and 53.");
            var result = await _schedullerController.GetAvailableSlots(invalidRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.IsNotNull(modelState);
            Assert.That(modelState.ContainsKey("WeekNumber"), Is.True);
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithValidationErrorDetails_WhenInvalidYear()
        {
            // Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int invalidYear = -1; //Year must be positive.
            var invalidRequest = new AvailableSlotsRequest
            {
                WeekNumber = currentWeek,
                Year = invalidYear
            };

            // Act
            _schedullerController.ModelState.AddModelError("Year", "The year must be a positive number.");
            var result = await _schedullerController.GetAvailableSlots(invalidRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.IsNotNull(modelState);
            Assert.That(modelState.ContainsKey("Year"), Is.True);

        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithValidationErrorDetails_WhenInvalidYearAndWeek()
        {
            // Arrange
            int invalidWeek = 60; //A year has between 1 and  53 weeks.
            int invalidYear = -1; //Year must be positive.
            var invalidRequest = new AvailableSlotsRequest
            {
                WeekNumber = invalidWeek,
                Year = invalidYear
            };

            // Act
            _schedullerController.ModelState.AddModelError("Year", "The year must be a positive number.");
            _schedullerController.ModelState.AddModelError("WeekNumber", "The week number must be between 1 and 53.");
            var result = await _schedullerController.GetAvailableSlots(invalidRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.IsNotNull(modelState);
            Assert.That(modelState.ContainsKey("Year"), Is.True);
            Assert.That(modelState.ContainsKey("WeekNumber"), Is.True);
        }


        [Test]
        public async Task GetAvailableSlots_ShouldReturnInternalServerError_WhenSchedulerService_ThrowsException()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(request.WeekNumber, request.Year)).ThrowsAsync(new Exception());

            //Act
            var result = await _schedullerController.GetAvailableSlots(request);

            //Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);

            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
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
        public async Task ScheduleAppointment_ShouldReturnBadRequest_WithInvalidRequest()
        {
            // Arrange
            AddModelStateErrors();
            var request = new AppointmentRequest(); 

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            var modelState = badRequestResult.Value as SerializableError;
            Assert.IsNotNull(modelState);
            AssertModelStateContainsKeys(modelState);
        }
        #region Helper Methods
        private void AddModelStateErrors()
        {
            _schedullerController.ModelState.AddModelError("Start", "Start time is required");
            _schedullerController.ModelState.AddModelError("End", "End time is required");
            _schedullerController.ModelState.AddModelError("FacilityId", "FacilityId is required");
            _schedullerController.ModelState.AddModelError("Comment", "The field Comment must be a string with a maximum length of 100.");
            _schedullerController.ModelState.AddModelError("PatientRequest.Name", "The field Name is required.");
            _schedullerController.ModelState.AddModelError("PatientRequest.SecondName", "The field SecondName is required.");
            _schedullerController.ModelState.AddModelError("PatientRequest.Email", "The field Email is required.");
            _schedullerController.ModelState.AddModelError("PatientRequest.Phone", "The field Phone is required.");
        }
        private void AssertModelStateContainsKeys(SerializableError modelState)
        {
            Assert.IsNotNull(modelState);
            Assert.That(modelState.ContainsKey("Start"), Is.True);
            Assert.That(modelState.ContainsKey("End"), Is.True);
            Assert.That(modelState.ContainsKey("FacilityId"), Is.True);
            Assert.That(modelState.ContainsKey("Comment"), Is.True);
            Assert.That(modelState.ContainsKey("PatientRequest.Name"), Is.True);
            Assert.That(modelState.ContainsKey("PatientRequest.SecondName"), Is.True);
            Assert.That(modelState.ContainsKey("PatientRequest.Email"), Is.True);
            Assert.That(modelState.ContainsKey("PatientRequest.Phone"), Is.True);
        }
        #endregion
        

        
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

        
        [Test]
        public async Task ScheduleAppointment_ShouldReturnInternalServerError_WhenSchedulerService_ThrowsException()
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

            
            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>())).ThrowsAsync(new Exception());

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);

            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        }
        
        #endregion
    }
}