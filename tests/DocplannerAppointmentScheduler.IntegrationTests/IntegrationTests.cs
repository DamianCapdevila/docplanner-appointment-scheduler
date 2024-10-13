using Moq;
using DocplannerAppointmentScheduler.Core.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using DocplannerAppointmentScheduler.Api.Models;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;

namespace DocplannerAppointmentScheduler.IntegrationTests
{
    public class IntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private Mock<ISchedulerService> _schedulerServiceMock;
        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
            _schedulerServiceMock = new Mock<ISchedulerService>();
        }

        [TearDown]
        public void Dispose()
        {
            _factory.Dispose();
        }

        #region GET AVAILABLE SLOTS
        [Test]
        public async Task GetAvailableSlots_ShouldReturnOk_WithValidRequestParameters()
        {
            //Arrange
            var client = _factory.CreateClient();

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            var requestParameters = $"WeekNumber={currentWeek}&Year={currentYear}";

            //Act
            var response = await client.GetAsync($"api/Scheduler/availableSlots?{requestParameters}");
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithInvalidWeekParameter()
        {
            //Arrange
            var client = _factory.CreateClient();

            int invalidWeek = 100;
            int currentYear = DateTime.Now.Year;
            var requestParameters = $"WeekNumber={invalidWeek}&Year={currentYear}";

            //Act
            var response = await client.GetAsync($"api/Scheduler/availableSlots?{requestParameters}");
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnBadRequest_WithInvalidYearParameter()
        {
            //Arrange
            var client = _factory.CreateClient();

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int invalidYear = 1990;
            var requestParameters = $"WeekNumber={currentWeek}&Year={invalidYear}";

            //Act
            var response = await client.GetAsync($"api/Scheduler/availableSlots?{requestParameters}");
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetAvailableSlots_ShouldReturnInternalServerError_OnException()
        {
            //Arrange
            var mockFactory = CreateMockFactory();
            var client = mockFactory.CreateClient();
            

            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            var requestParameters = $"WeekNumber={currentWeek}&Year={currentYear}";

            

            //Act
            var response = await client.GetAsync($"api/Scheduler/availableSlots?{requestParameters}");
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        #endregion

        #region SCHEDULE APPOINTMENT
        public async Task ScheduleAppointment_ShouldReturnCreated_WithValidRequestParameters()
        {
            //Arrange
            var client = _factory.CreateClient();

            var validAppointmentRequest = new AppointmentRequest
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

            var jsonContent = new StringContent(JsonConvert.SerializeObject(validAppointmentRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("api/Scheduler/scheduleAppointment", jsonContent);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }


        [Test]
        public async Task ScheduleAppointment_ShouldReturnBadRequest_WithInvalidRequestParameters()
        {
            //Arrange
            var client = _factory.CreateClient();

            var validAppointmentRequest = new AppointmentRequest
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
                    Phone = "" //Phone can not be empty
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(validAppointmentRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("api/Scheduler/scheduleAppointment", jsonContent);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

       
        [Test]
        public async Task ScheduleAppointment_ShouldReturnInternalServerError_OnException()
        {
            //Arrange
            var mockFactory = CreateMockFactory();
            var client = mockFactory.CreateClient();


            var validAppointmentRequest = new AppointmentRequest
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
                    Phone = "+346658797979" 
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(validAppointmentRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("api/Scheduler/scheduleAppointment", jsonContent);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        #endregion 


        private WebApplicationFactory<Program> CreateMockFactory()
        {
            var mockSchedulerService = new Mock<ISchedulerService>();
            mockSchedulerService.Setup(s => s.GetAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception());

            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddScoped<ISchedulerService>(provider => mockSchedulerService.Object);
                    });
                });
        }




    }
}