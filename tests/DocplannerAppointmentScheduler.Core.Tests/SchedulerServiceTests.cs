using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.TestUtilities.DataBuilders;
using Moq;
using System.Net;
namespace DocplannerAppointmentScheduler.Core.Tests
{
    public class SchedulerServiceTests
    {
        private Mock<IAvailabilityService> _availabilityServiceMock;
        private SchedulerService _schedulerService;
        
        [SetUp]
        public void Setup()
        {
            _availabilityServiceMock = new Mock<IAvailabilityService>();
            _schedulerService = new SchedulerService(_availabilityServiceMock.Object);
        }

        #region GET AVAILABLE SLOTS
        [Test]
        public async Task GetAvailableSlots_ShouldCall_AvailabilityService_GetAvailableSlotsAsync()
        {
            _availabilityServiceMock.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new WeeklyAvailabilityDTO());

            //Act
            var availableSlots = await _schedulerService.GetAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<int>());

            //Assert
            _availabilityServiceMock.Verify(s => s.GetWeeklyAvailabilityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public async Task GetAvailableSlotsAsync_ShouldReturnWeeklyAvailability_WhenAvailabilityServiceReturnsWeeklyAvailability()
        {
            //Arrange
            int slotDurationMinutes = 10;
            int ammountFreeSlotsPerDay = 10;

            var fakeDataGenerator = new FakeDataGenerator();
            var fakeWeeklyAvailability = fakeDataGenerator.GenerateFakeWeeklyAvailability(slotDurationMinutes, ammountFreeSlotsPerDay);


            _availabilityServiceMock.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fakeWeeklyAvailability);
            
            //Act
            var availableSlots = await _schedulerService.GetAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<int>());

            //Assert
            Assert.That(availableSlots, Is.EqualTo(fakeWeeklyAvailability));
        }

        [Test]
        public void GetAvailableSlotsAsync_ShouldThrowException_WhenAvailabilityServiceThrowsException()
        {
            //Arrange
            _availabilityServiceMock.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception());

            //Assert
            //Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _schedulerService.GetAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<int>()));
        }

        #endregion

        #region SCHEDULE APPOINTMENT
        [Test]
        public async Task GetAvailableSlots_ShouldCall_AvailabilityService_TakeSlotAsync()
        {
            //Arrange
            _availabilityServiceMock.Setup(s => s.TakeSlotAsync(It.IsAny<AppointmentRequestDTO>())).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            //Act
            var appointmentScheduled = await _schedulerService.ScheduleAppointmentAsync(It.IsAny<AppointmentRequestDTO>());

            //Assert
            _availabilityServiceMock.Verify(s => s.TakeSlotAsync(It.IsAny<AppointmentRequestDTO>()), Times.Once);
        }
        [Test]
        public async Task ScheduleAppointmentAsync_ShouldReturnSameResponse_Than_AvailabilityServiceResponse()
        {
            //Arrange
            var fakeDataGenerator = new FakeDataGenerator(); 
            var randomResponseMessage = fakeDataGenerator.GenerateFakeHttpResponse();


            _availabilityServiceMock.Setup(s => s.TakeSlotAsync(It.IsAny<AppointmentRequestDTO>())).ReturnsAsync(randomResponseMessage);

            //Act
            var appointmentScheduled = await _schedulerService.ScheduleAppointmentAsync(new AppointmentRequestDTO());

            //Assert
            Assert.That(appointmentScheduled, Is.EqualTo(randomResponseMessage));
        }

        [Test]
        public void ScheduleAppointmentAsync_ShouldThrowException_WhenAvailabilityServiceThrowsException()
        {
            //Arrange
            _availabilityServiceMock.Setup(s => s.TakeSlotAsync(It.IsAny<AppointmentRequestDTO>())).ThrowsAsync(new Exception());

            //Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _schedulerService.ScheduleAppointmentAsync(new AppointmentRequestDTO()));
        }
        #endregion
    }
}