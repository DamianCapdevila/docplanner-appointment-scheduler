using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Api.Models;
using DocplannerAppointmentScheduler.Api.Infrastructure;

namespace DocplannerAppointmentScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger _logger;

        public SchedulerController(ISchedulerService schedulerService, ILogger<SchedulerController> logger)
        {
            _schedulerService = schedulerService;
            _logger = logger;
        }

        [HttpGet("availableSlots")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WeeklyAvailabilityDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] AvailableSlotsRequest request)
        {
            var result = await _schedulerService.GetAvailableSlotsAsync(request.WeekNumber, request.Year);
            return result.Match(
                    Ok,
                    serviceError => this.HandleServiceError(_logger, serviceError,
                    $"getting available slots for week {request.WeekNumber}, year {request.Year}.")
            );
        }

        [HttpPost("scheduleAppointment")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ScheduleAppointment([FromBody] AppointmentRequest request)
        {
            var result = await _schedulerService.ScheduleAppointmentAsync(request);
            return result.Match(
                    _ => Created("/appointments/{id}", $"Appointment created at {request.Start}"),
                    serviceError => this.HandleServiceError(_logger, serviceError,
                    $"Scheduling appointment at {request.Start}")
            );
        }
    }
}
