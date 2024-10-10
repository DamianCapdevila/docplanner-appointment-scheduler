using AutoMapper;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.Tests.DataBuilders;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
        public async Task TakeSlotAsync_ShouldReturnTrue_When_ExternalServiceResponse_IsSuccessStatusCode()
        {
            //Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*")
                    .Respond("application/json", "{'availability' : 'Availability Info'}");
            

            var client = mockHttp.ToHttpClient();

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                          .Returns(client);

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeAppointmentRequest = fakeDataGenerator.GenerateFakeAppointmentRequest();

            //Act
            var slotTaken = await _availabilityService.TakeSlotAsync(fakeAppointmentRequest);

            //Assert
            Assert.That(slotTaken, Is.True);
        }
        #endregion

    }
}
