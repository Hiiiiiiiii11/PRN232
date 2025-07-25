using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Appointment;
using Services.DTOs.Consultant;
using Services.IService;
using System.Security.Claims;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantController : ControllerBase
    {
        private IConsultantService _consultantService;
        public ConsultantController(IConsultantService consultantService)
        {
            _consultantService = consultantService;
        }
        [HttpGet("WorkingHour/{id}")]
        public async Task<IActionResult> GetWorkingHours([FromRoute]int id)
        {
            try
            {
                var result = await _consultantService.GetWorkingHour(id);
                if (result == null )
                {
                    return BadRequest(new { message = "Id invalid" });
                }
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateConsultant([FromBody] ConsultantRequest consultantRequest)
        {
            try
            {
                var result = await _consultantService.CreateConsultant(consultantRequest);
                if (result == null)
                {
                    return BadRequest(new { message = "Id invalid" });
                }
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
