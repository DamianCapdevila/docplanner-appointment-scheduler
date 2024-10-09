using AutoMapper;
using DocplannerAppointmentScheduler.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocplannerAppointmentScheduler.Core.Tests
{
    public class AvailabilityServiceTests
    {
        private Mock<IMapper> _mapperMock;
        private AvailabilityService _availabilityService;

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();
            _availabilityService = new AvailabilityService(_mapperMock.Object);
        }

        [Test]
        public async Task TakeSlotAsync_ShouldReturnFalse_WhenNoSuccess_WithNoExceptions()
        { 
        }
    }
}
