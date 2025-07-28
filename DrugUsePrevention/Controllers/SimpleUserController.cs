using Microsoft.AspNetCore.Mvc;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleUserController : ControllerBase
    {
        /// <summary>
        /// Test endpoint to verify API is working
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "User API is working!", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Get user by ID - placeholder
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            return Ok(new { userId = id, message = "User endpoint placeholder" });
        }
    }
} 