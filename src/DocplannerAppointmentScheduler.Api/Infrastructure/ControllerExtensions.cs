using Microsoft.AspNetCore.Mvc;
using DocplannerAppointmentScheduler.Core.Results;

namespace DocplannerAppointmentScheduler.Api.Infrastructure
{
    public static class ControllerExtensions
    {
        public static IActionResult HandleServiceError(
            this ControllerBase controller,
            ILogger logger,
            Error error,
            string context)
        {
            string details = $"External service error while {context}: {error.Message}";
            logger.LogWarning(details);

            return controller.StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service unavailable.",
                Detail = details,
                Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.4"
            });
        }
    }
}
