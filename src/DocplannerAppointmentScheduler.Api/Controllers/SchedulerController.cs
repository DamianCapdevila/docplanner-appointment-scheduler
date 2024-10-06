using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Models;
using Microsoft.VisualBasic;
using System.Globalization;

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
        public async Task<IActionResult> GetAvailableSlots([FromQuery]int weekNumber, [FromQuery]int year)
        {
            try
            {
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
        public async Task<IActionResult> ScheduleAppointment([FromBody] ScheduleAppointmentRequest request)
        {
            //If any of the rules enforced in the ScheduleAppointmentRequest is broken, we should inform the Api user that the
            //Request has the incorrect body, hence we return a 400, with the errors serialized so the client will know what´s wrong. 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appointmentRequest = new AppointmentRequestDTO
                {
                    Slot = new FreeSlotDTO { Start = request.Start, End = request.End },
                    FacilityId = request.FacilityId,
                    Comment = request.Comment,
                    Patient = new PatientDTO
                    {
                        Name = request.PatientRequest.Name,
                        SecondName = request.PatientRequest.SecondName,
                        Email = request.PatientRequest.Email,
                        Phone = request.PatientRequest.Phone
                    }

                };
                var appointmentScheduled = await _schedulerService.ScheduleAppointment(appointmentRequest);
                return Ok(appointmentScheduled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling an appointment at {Start}", request.Start);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while scheduling an appointment" });
            }
        }
    }
}
