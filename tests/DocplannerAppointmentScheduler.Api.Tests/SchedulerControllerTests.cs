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

        #region Data passed to the controller is valid, we check the availability
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
            

            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(currentWeek, currentYear))
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

            _schedulerServiceMock.Setup(s => s.GetAvailableSlots(currentWeek, currentYear))
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
        #endregion


        #region Data passed to the controller is invalid, we check that meaningful errors are returned
        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WhenSelectedWeekIsInThePast()
        {
            // Arrange
            int pastWeek = ISOWeek.GetWeekOfYear(DateTime.Now) - 1;
            int currentYear = DateTime.Now.Year;
            AvailableSlotsRequest request = new AvailableSlotsRequest { WeekNumber = pastWeek, Year = currentYear };

            ValidateModel(request);
            // Act
            var result = await _schedullerController.GetAvailableSlots(request);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WhenInvalidWeekAndOrYear()
        {
            // Arrange
            int invalidWeek = 100;  // Invalid week: a year never has 100 weeks
            int invalidYear = -1;    // Invalid year: could never be negative
            var invalidRequest = new AvailableSlotsRequest
            {
                WeekNumber = invalidWeek,
                Year = invalidYear
            };

            // Act
            ValidateModel(invalidRequest);
            var result = await _schedullerController.GetAvailableSlots(invalidRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            var modelStateDict = badRequestResult.Value as SerializableError;

            Assert.Multiple(() =>
            {
                Assert.That(modelStateDict.ContainsKey("WeekNumber"), Is.True, "WeekNumber validation error should be present");
                Assert.That(modelStateDict.ContainsKey("Year"), Is.True, "Year validation error should be present");
            });
        }

        private void ValidateModel(AvailableSlotsRequest model)
        {
            _schedullerController.ModelState.Clear();

            // Standard property validations
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();

            // Handle basic range validation for Year
            if (model.Year <= 0)
            {
                _schedullerController.ModelState.AddModelError(
                    nameof(model.Year),
                    "The year must be a positive number.");
            }

            // Handle basic range validation for WeekNumber
            if (model.WeekNumber < 1 || model.WeekNumber > 53)
            {
                _schedullerController.ModelState.AddModelError(
                    nameof(model.WeekNumber),
                    "The week number must be between 1 and 53.");
            }

            // Handle FutureWeek validation only if Year and Week are within valid ranges
            if (model.Year > 0 && model.WeekNumber >= 1 && model.WeekNumber <= 53)
            {
                try
                {
                    var futureWeekAttribute = new FutureWeekAttribute();
                    var result = futureWeekAttribute.GetValidationResult(model, validationContext);
                    if (result != null)
                    {
                        _schedullerController.ModelState.AddModelError(
                            nameof(model.WeekNumber),
                            result.ErrorMessage);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // If ISOWeek.ToDateTime throws, we already have the basic range validation errors
                }
            }
        }

        #endregion
    }
}