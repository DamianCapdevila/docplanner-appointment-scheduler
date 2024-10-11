using AutoMapper;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;
using Moq;
using RichardSzalay.MockHttp;
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
        public async Task TakeSlotAsync_ShouldReturnShouldReturnSameResponse_Than_ExternalAvailabilityService()
        {
            //Arrange
            var fakeDataGenerator = new FakeDataGenerator();
            var randomResponseMessage = fakeDataGenerator.GenerateFakeHttpResponse();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(req => randomResponseMessage);
            

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequest();

            //Act
            var result = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(result.StatusCode, Is.EqualTo(randomResponseMessage.StatusCode));
        }

        [Test]
        public void TakeSlotAsync_ShouldThrowException_When_ExternalAvailabilityService_ThrowsException()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Throw(new Exception());

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequest();

            //Act && Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _availabilityService.TakeSlotAsync(fakeAppointmentRequest));
        }

        #endregion

    }
}
