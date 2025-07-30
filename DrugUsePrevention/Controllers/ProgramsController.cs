using Microsoft.AspNetCore.Mvc;
using Services.IService;
using BussinessObjects;
using Services.DTOs.Program.DrugUsePrevention.DTOs;
using Services.DTOs.Program;
using Services.DTOs.ProgramsPacitination;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _service;

        public ProgramsController(IProgramService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var programs = await _service.GetAllProgramsAsync();
            var result = programs.Select(p => new ProgramResponseDto
            {
                ProgramID = p.ProgramID,
                Title = p.Title,
                Description = p.Description,
                ThumbnailURL = p.ThumbnailURL,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Location = p.Location,
                CreatedBy = p.CreatedBy,
                IsActive = p.IsActive
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var program = await _service.GetProgramByIdAsync(id);
            if (program == null) return NotFound();

            var dto = new ProgramDetailDto
            {
                ProgramID = program.ProgramID,
                Title = program.Title,
                Description = program.Description,
                ThumbnailURL = program.ThumbnailURL,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                Location = program.Location,
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProgramRequestDto request)
        {
            var program = new Program
            {
                Title = request.Title,
                Description = request.Description,
                ThumbnailURL = request.ThumbnailURL,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                CreatedBy = request.CreatedBy,
                IsActive = request.IsActive
            };

            await _service.AddProgramAsync(program);

            var response = new ProgramResponseDto
            {
                ProgramID = program.ProgramID,
                Title = program.Title,
                Description = program.Description,
                ThumbnailURL = program.ThumbnailURL,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                Location = program.Location,
                CreatedBy = program.CreatedBy,
                IsActive = program.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = program.ProgramID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramRequestDto request)
        {
            var existing = await _service.GetProgramByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.ThumbnailURL = request.ThumbnailURL;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.Location = request.Location;
            existing.CreatedBy = request.CreatedBy;
            existing.IsActive = request.IsActive;

            await _service.UpdateProgramAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteProgramAsync(id);
            return NoContent();
        }
    }
}
