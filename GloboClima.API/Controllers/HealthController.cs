using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Simple controller used for health checks of the API service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns OK if the service is running.
    /// </summary>
    /// <response code="200">Service is healthy.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok("Healthy");
    }
}
