using System.Security.Claims;
using BussinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Appointment;
using Services.DTOs.User;
using Services.IService;
using Services.MailUtils;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment(
            [FromForm] AppointmentRequest appointmentRequest
        )
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _appointmentService.CreateAppointment(
                    appointmentRequest,
                    userId
                );
                if (result == null)
                {
                    return BadRequest(new { message = "Please Login" });
                }
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointment()
        {
            try
            {
                var result = await _appointmentService.GetAllAppointment();

                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
