using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.DTOs;
using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Api.Models;

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
        public async Task<IActionResult> GetAvailableSlots([FromQuery]DateTime date)
        {
            
            try
            {
                var availableSlots = await _schedulerService.GetAvailableSlots(date);

                var response = new AvailableSlotsResponse
                {
                    Date = date,
                    Slots = availableSlots.Select(slot => new SlotResponse
                    {
                        Start = slot.Start,
                        End = slot.End
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available slots for date {Date}", date);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving slots"});
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
                    Slot = new TimeSlotDTO { Start = request.Start, End = request.End },
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
