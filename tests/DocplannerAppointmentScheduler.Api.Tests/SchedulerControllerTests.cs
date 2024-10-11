using Microsoft.Extensions.Logging;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Controllers;
using DocplannerAppointmentScheduler.Api.Models;
using Moq;
using DocplannerAppointmentScheduler.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using DocplannerAppointmentScheduler.Api.Validators;
using System.Net;


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

            int numberOfAvailableSlots = 2;
            uint ammountDaySchedules = 1;

            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);


            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(weeklyAvailability);

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
            Assert.That(response.DaySchedules.Sum(ds => ds.AvailableSlots.Count), Is.EqualTo(numberOfAvailableSlots));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithEmptySlots_WhenNoFreeSlotsAvailable()
        {
            // Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            int numberOfAvailableSlots = 0;
            uint ammountDaySchedules = 7;

            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);

            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(currentWeek, currentYear))
                .ReturnsAsync(weeklyAvailability);

            // Act
            var result = await _schedullerController.GetAvailableSlots(request);

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
        public async Task GetAvailableSlots_ShouldReturnServiceUnavailable_WhenSchedulerService_ThrowsHttpRequestException()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            int numberOfAvailableSlots = 2;
            uint ammountDaySchedules = 1;
            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);


            _schedulerServiceMock.Setup(s => s.GetAvailableSlotsAsync(request.WeekNumber, request.Year)).ThrowsAsync(new HttpRequestException());

            //Act
            var result = await _schedullerController.GetAvailableSlots(request);

            //Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);

            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.ServiceUnavailable));
        }


        [Test]
        public async Task GetAvailableSlots_ShouldReturnInternalServerError_WhenSchedulerService_ThrowsException()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = currentWeek, Year = currentYear };

            int numberOfAvailableSlots = 2;
            uint ammountDaySchedules = 1;
            var weeklyAvailability = CreateWeeklyAvailability(numberOfAvailableSlots, ammountDaySchedules);


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
        public async Task ScheduleAppointment_ShouldReturnOk_WithValidRequest()
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

            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>())).ReturnsAsync(true);

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

            // Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }
        

        
        public async Task ScheduleAppointment_ShouldReturnBadRequest_WithInvalidRequest()
        {
            // Arrange
            AddModelStateErrors();

            var request = new AppointmentRequest(); // Invalid request with missing data -> Not relevant here, as the bad request is triggered by invalid model.

            // Act
            var result = await _schedullerController.ScheduleAppointment(request);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));

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
        public async Task ScheduleAppointment_ShouldReturnServiceUnavailable_WhenSchedulerService_ReturnsFalse()
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

            //It means that appointment could not be made .... IMPROVEMENTS HAVE TO BE MADE IN THE LOGIC DOWN IN THE SERVICE LAYER.
            _schedulerServiceMock.Setup(s => s.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>())).ReturnsAsync(false);

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