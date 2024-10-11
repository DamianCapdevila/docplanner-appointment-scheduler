using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Api.Models;
using AutoMapper;

namespace DocplannerAppointmentScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SchedulerController(ISchedulerService schedulerService, ILogger<SchedulerController> logger, IMapper mapper)
        {
            _schedulerService = schedulerService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("availableSlots")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WeeklyAvailabilityDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] AvailableSlotsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var availableSlots = await _schedulerService.GetAvailableSlotsAsync(request.WeekNumber, request.Year);
                return Ok(availableSlots);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "External service error getting available slots for week {WeekNumber}, year {Year}.", request.WeekNumber, request.Year);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Error getting weekly availability from the external availability service." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error getting available slots for week {WeekNumber}, year {Year}.", request.WeekNumber, request.Year);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while retrieving available slots." });
            }
        }


        [HttpPost("scheduleAppointment")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string[]))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ScheduleAppointment([FromBody] AppointmentRequest request)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appointmentRequest = _mapper.Map<AppointmentRequestDTO>(request);
                var response = await _schedulerService.ScheduleAppointmentAsync(appointmentRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "Appointment scheduled successfully!" });
                }
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "An error occurred in the external availability service when scheduling an appointment." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling an appointment at {Start}", request.Start);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while scheduling an appointment" });
            }
        }
    }
}
