using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Api.Models;
using System.Globalization;
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
        public async Task<IActionResult> GetAvailableSlots([FromQuery]int weekNumber, [FromQuery]int year)
        {
            try
            {
                //Validation to ensure the requested week is the current one or a week in the future.
                var currentDate = DateTime.Now; 
                var mondayInSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
                var sundayInSelectedWeek = mondayInSelectedWeek.AddDays(6);
                var selectedWeekIsInThePast = currentDate.Date > sundayInSelectedWeek;

                if (selectedWeekIsInThePast)
                {
                    return BadRequest(new { message = "The selected week has already passed. Please choose a future week." });
                }

                var availableSlots = await _schedulerService.GetAvailableSlots(weekNumber, year);
                return Ok(availableSlots);
            }
            catch(HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting available slots for week {WeekNumber}, year {Year}.", weekNumber, year);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Error getting weekly availability from the external availability service." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available slots for week {WeekNumber}, year {Year}.", weekNumber, year);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving available slots."});
            }
        }

        [HttpPost("scheduleAppointment")]
        public async Task<IActionResult> ScheduleAppointment([FromBody] AppointmentRequest request)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appointmentRequest = _mapper.Map<AppointmentRequestDTO>(request);
                var appointmentScheduled = await _schedulerService.ScheduleAppointment(appointmentRequest);
                
                if (appointmentScheduled)
                {
                    return Ok(new { message = "Appointment scheduled successfully!" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while scheduling an appointment" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling an appointment at {Start}", request.Start);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while scheduling an appointment" });
            }
        }
    }
}
