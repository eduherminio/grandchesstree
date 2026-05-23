using Microsoft.AspNetCore.Mvc;

namespace GrandChessTree.Api.Health
{
    [Route("api/v1/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Return an OK status indicating the app is healthy
            return Ok(new { status = "Healthy", message = "API is up and running!" });
        }
    }
}
