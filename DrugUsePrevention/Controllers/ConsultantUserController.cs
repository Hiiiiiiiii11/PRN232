using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.DTOs.Common;
using Services.DTOs.ConsultantUser;
using Services.DTOs.Courses;
using Services.IService;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantUserController : ControllerBase
    {
        private readonly IConsultantUserService _consultantUserService;

        public ConsultantUserController(IConsultantUserService consultantUserService)
        {
            _consultantUserService = consultantUserService;
        }

        #region ✅ API View Consultants - PUBLIC ACCESS (Guest có thể xem)

        /// <summary>
        /// Get all active consultants with pagination and filtering
        /// PUBLIC: Guest, Member đều có thể xem danh sách consultant
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<ConsultantDto>>> GetConsultants(
            [FromQuery] ConsultantFilterDto filter
        )
        {
            try
            {
                var result = await _consultantUserService.GetAllActiveConsultantsAsync(filter);
                return Ok(PaginatedApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get all consultants for admin (including inactive)
        /// ADMIN: Staff, Manager, Admin có thể xem tất cả consultant
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<
            ActionResult<PaginatedApiResponse<ConsultantDto>>
        > GetAllConsultantsForAdmin([FromQuery] ConsultantFilterDto filter)
        {
            try
            {
                var result = await _consultantUserService.GetAllConsultantsAsync(filter);
                return Ok(PaginatedApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get consultant details by ID
        /// PUBLIC: Guest có thể xem chi tiết consultant
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> GetConsultant(int id)
        {
            try
            {
                var result = await _consultantUserService.GetConsultantByIdAsync(id);
                return Ok(ApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get consultant by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ConsultantDto>>> GetConsultantByUserId(
            int userId
        )
        {
            try
            {
                var result = await _consultantUserService.GetConsultantByUserIdAsync(userId);
                return Ok(ApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get consultants by specialty
        /// </summary>
        [HttpGet("specialty")]
        [AllowAnonymous]
        public async Task<
            ActionResult<PaginatedApiResponse<ConsultantDto>>
        > GetConsultantsBySpecialty(
            [FromQuery] string specialty,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _consultantUserService.GetConsultantsBySpecialtyAsync(
                    specialty,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Search consultants
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<ConsultantDto>>> SearchConsultants(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _consultantUserService.SearchConsultantsAsync(
                    searchKeyword,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<ConsultantDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get consultant's courses
        /// PUBLIC: Có thể xem danh sách khóa học của consultant
        /// </summary>
        [HttpGet("{consultantId}/courses")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<CourseDto>>> GetConsultantCourses(
            int consultantId,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _consultantUserService.GetConsultantCoursesAsync(
                    consultantId,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<CourseDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion



        /// <summary>
        /// Toggle consultant status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleConsultantStatus(
            int id,
            [FromBody] string status
        )
        {
            try
            {
                int updatedBy = GetCurrentUserId();
                await _consultantUserService.UpdateConsultantStatusAsync(id, status, updatedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật trạng thái thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        #region Statistics

        /// <summary>
        /// Get consultant statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<ConsultantStatsDto>>> GetConsultantStats()
        {
            try
            {
                var result = await _consultantUserService.GetConsultantStatsAsync();
                return Ok(ApiResponse<ConsultantStatsDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }

        #endregion
    }
}
