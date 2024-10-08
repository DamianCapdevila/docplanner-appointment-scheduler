using DocplannerAppointmentScheduler.Core.Services;
using Moq;
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


        [Test]
        public async Task TakeSlot_ShouldReturnFalse_WhenAppointmentCouldNotBeMadeWithNoErrors()
        {

        }

        [Test]
        public async Task TakeSlot_ShouldReturnTrue_WhenExternalAvailabilityServiceReturnsOk()
        {

        }

        [Test]
        public async Task TakeSlot_ShouldReturnFalse_WhenAvailabilityServiceThrowsException()
        {

        }
    }
}