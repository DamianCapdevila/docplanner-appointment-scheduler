using AutoMapper;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Domain;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Net;

namespace DocplannerAppointmentScheduler.Core.Tests
{
    public class AvailabilityServiceTests
    {
        private Mock<IMapper> _mapperMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private AvailabilityService _availabilityService;
       

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _availabilityService = new AvailabilityService(_mapperMock.Object, _httpClientFactoryMock.Object);
        }

        #region TAKE SLOT
        [Test]
        public async Task TakeSlotAsync_ShouldReturnSameResponse_Than_ExternalAvailabilityService_When_ExternalAvailabilityService_ReturnsSuccess()
        {
            //Arrange
            var fakeDataGenerator = new FakeDataGenerator();
            var randomResponseMessage = fakeDataGenerator.GenerateFakeHttpResponse(range: TestUtilities.Enums.StatusCodeRange.Success);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(req => randomResponseMessage);
            

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act
            var result = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(result.StatusCode, Is.EqualTo(randomResponseMessage.StatusCode));
        }

        [Test]
        public async Task TakeSlotAsync_ShouldReturnInternalServerError_When_ExternalAvailabilityService_ThrowsException()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Throw(new Exception());

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError)); 
        }

        [Test]
        public async Task TakeSlotAsync_ShouldReturnBadRequest_When_ExternalAvailabilityService_ReturnsBadRequest()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.BadRequest);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task TakeSlotAsync_ShouldReturnUnauthorized_When_ExternalAvailabilityService_ReturnsUnauthorized()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.Unauthorized);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task TakeSlotAsync_ShouldReturnNotFound_When_ExternalAvailabilityService_ReturnsNotFound()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.NotFound);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        #endregion

        #region GET WEEKLY AVAILABILITY
        [Test]
        public async Task GetWeeklyAvailabilityAsync_ShouldReturnOk_When_ExternalAvailabilityService_ReturnsOkWithValidData()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;
            DateTime mondayOfThisWeek = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);


            var fakeDataGenerator = new FakeDataGenerator();
            var fakeOccupancyData = fakeDataGenerator.GenerateFakeFacilityOccupancyDTO(slotDurationMinutes: 10, busySlotsPerDay: 1, mondayOfThisWeek);

            var fakeFacilityOccupancy = new FacilityOccupancy() { Facility = new Facility() {Name = "Name", Address = "Address" } };  
            var fakeWeeklyAvailability = new WeeklyAvailability(fakeFacilityOccupancy);
            var fakeWeeklyAvailabilityDto = new WeeklyAvailabilityDTO();  

            var fakeOccupancyJson = JsonConvert.SerializeObject(fakeOccupancyData);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond("application/json", fakeOccupancyJson);


            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            
            _mapperMock.Setup(m => m.Map<FacilityOccupancy>(It.IsAny<FacilityOccupancyDTO>())).Returns(fakeFacilityOccupancy);
            _mapperMock.Setup(m => m.Map<WeeklyAvailabilityDTO>(It.IsAny<WeeklyAvailability>())).Returns(fakeWeeklyAvailabilityDto);

            //Act
            var result = await _availabilityService.GetWeeklyAvailabilityAsync(currentWeek, currentYear);

            //Assert
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetWeeklyAvailabilityAsync_ShouldReturnInternalServerError_When_ExternalAvailabilityService_ThrowsException()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Throw(new Exception());

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.GetWeeklyAvailabilityAsync(currentWeek, currentYear);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task GetWeeklyAvailabilityAsync_ShouldReturnBadRequest_When_ExternalAvailabilityService_ReturnsBadRequest()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.BadRequest);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.GetWeeklyAvailabilityAsync(currentWeek, currentYear);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetWeeklyAvailabilityAsync_ShouldReturnUnauthorized_When_ExternalAvailabilityService_ReturnsUnauthorized()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.Unauthorized);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.GetWeeklyAvailabilityAsync(currentWeek, currentYear);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetWeeklyAvailabilityAsync_ShouldReturnNotFound_When_ExternalAvailabilityService_ReturnsNotFound()
        {
            //Arrange
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            int currentYear = DateTime.Now.Year;

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.NotFound);

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequestDTO();

            //Act 
            var response = await _availabilityService.GetWeeklyAvailabilityAsync(currentWeek, currentYear);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        #endregion

    }
}
