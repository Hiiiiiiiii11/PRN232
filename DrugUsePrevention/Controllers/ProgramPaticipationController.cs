using BussinessObjects;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.ProgramsPacitination;
using Services.IService;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramParticipationController : ControllerBase
    {
        private readonly IProgramParticipationService _service;

        public ProgramParticipationController(IProgramParticipationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var participations = await _service.GetByUserIdAsync(userId);

            var response = participations.Select(p => new ProgramParticipationResponse
            {
                ParticipationID = p.ParticipationID,
                UserID = p.UserID,
                ProgramID = p.ProgramID,
                ParticipatedAt = p.ParticipatedAt,
                UserFullName = p.User?.FullName,
                ProgramTitle = p.Program?.Title,
                Program = new ProgramDetailDto
                {
                    ProgramID = p.Program.ProgramID,
                    Title = p.Program.Title,
                    Description = p.Program.Description,
                    ThumbnailURL = p.Program.ThumbnailURL,
                    StartDate = p.Program.StartDate,
                    EndDate = p.Program.EndDate,
                    Location = p.Program.Location
                }
            });

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProgramParticipationCreateRequest request)
        {
            var participation = new ProgramParticipation
            {
                UserID = request.UserID,
                ProgramID = request.ProgramID,
                ParticipatedAt = DateTime.UtcNow
            };

            var created = await _service.AddAsync(participation);

            var response = new ProgramParticipationResponse
            {
                ParticipationID = created.ParticipationID,
                UserID = created.UserID,
                ProgramID = created.ProgramID,
                ParticipatedAt = created.ParticipatedAt,
                UserFullName = created.User?.FullName, // nếu bạn include
                ProgramTitle = created.Program?.Title
            };

            return CreatedAtAction(nameof(GetById), new { id = response.ParticipationID }, response);
        }

    }

}
