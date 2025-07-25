using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Common;
using Services.DTOs.CourseContent;
using Services.DTOs.Courses;
using Services.DTOs.Dashboard;
using Services.DTOs.Registration;
using Services.IService;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        #region ✅ API View Course - PUBLIC ACCESS (Guest có thể xem)

        /// <summary>
        /// [TASK] API View Course - Có phân trang, filter theo ngày tháng, kỹ năng, độ tuổi
        /// PUBLIC: Guest, Member đều có thể xem danh sách khóa học
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ✅ FIXED: Guest có thể xem danh sách khóa học
        public async Task<ActionResult<PaginatedApiResponse<CourseListDto>>> GetCourses(
            [FromQuery] CourseFilterDto filter
        )
        {
            try
            {
                var result = await _courseService.GetCoursesAsync(filter);
                return Ok(PaginatedApiResponse<CourseListDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get course details by ID
        /// PUBLIC: Guest có thể xem chi tiết khóa học công khai
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ FIXED: Guest có thể xem chi tiết khóa học
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> GetCourse(int id)
        {
            try
            {
                var result = await _courseService.GetCourseByIdAsync(id);
                return Ok(ApiResponse<CourseResponseDto>.SuccessResult(result));
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

        #endregion

        #region ✅ API Create Course - Cho Staff, Consultant, Manager, Admin

        /// <summary>
        /// [TASK] API Create khóa học - Cho phép tạo khóa học, cho phép up ảnh thumbnail của khóa học
        /// Staff: Quản lý chương trình và nội dung
        /// Consultant: Tạo khóa học chuyên môn
        /// Manager: Quản lý tổng thể
        /// Admin: Toàn quyền
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> CreateCourse(
            [FromBody] CreateCourseDto createDto
        )
        {
            try
            {
                int createdBy = GetCurrentUserId();

                var result = await _courseService.CreateCourseAsync(createDto, createdBy);
                return CreatedAtAction(
                    nameof(GetCourse),
                    new { id = result.CourseID },
                    ApiResponse<CourseResponseDto>.SuccessResult(result, "Tạo khóa học thành công")
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
#if DEBUG
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Lỗi server: {ex.Message}"));
#else
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo khóa học"));
#endif
            }
        }

        #endregion

        #region ✅ API Update/Delete Course - Cho Staff, Consultant, Manager, Admin

        /// <summary>
        /// [TASK] API Update Course - Cho Staff, Consultant, Manager, Admin
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> UpdateCourse(
            int id,
            [FromBody] UpdateCourseDto updateDto
        )
        {
            try
            {
                if (id != updateDto.CourseID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                int updatedBy = GetCurrentUserId();
                var result = await _courseService.UpdateCourseAsync(updateDto, updatedBy);
                return Ok(
                    ApiResponse<CourseResponseDto>.SuccessResult(
                        result,
                        "Cập nhật khóa học thành công"
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// [TASK] API Delete Course - Xóa khóa học bằng cách thay đổi trạng thái isActive = false
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<string>>> DeleteCourse(int id)
        {
            try
            {
                int deletedBy = GetCurrentUserId();
                await _courseService.DeleteCourseAsync(id, deletedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa khóa học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle course status (active/inactive)
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<string>>> ToggleCourseStatus(
            int id,
            [FromBody] bool isActive
        )
        {
            try
            {
                int updatedBy = GetCurrentUserId();
                await _courseService.ToggleCourseStatusAsync(id, isActive, updatedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật trạng thái thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Lỗi server: {ex.Message}"));
            }
        }

        #endregion

        #region ✅ API Manager Approval - Chỉ Manager và Admin

        /// <summary>
        /// [TASK] API cho phép Manager duyệt khóa học - isAccept = true
        /// Chỉ Manager và Admin mới có quyền duyệt khóa học
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")] // ✅ FIXED: Thêm Admin vào quyền approve
        public async Task<ActionResult<ApiResponse<string>>> ApproveCourse(
            int id,
            [FromBody] bool isAccept
        )
        {
            try
            {
                int approvedBy = GetCurrentUserId();
                await _courseService.ApproveCourseAsync(id, isAccept, approvedBy);

                string message = isAccept
                    ? "Duyệt khóa học thành công"
                    : "Từ chối khóa học thành công";
                return Ok(ApiResponse<string>.SuccessResult("", message));
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

        #endregion

        #region ✅ Course Contents Management

        /// <summary>
        /// [TASK] API list ra chi tiết các bài học trong course
        /// Hiển thị danh sách học phần và các bài học của course
        /// PUBLIC: Guest có thể xem nội dung khóa học công khai
        /// </summary>
        [HttpGet("{courseId}/contents")]
        [AllowAnonymous] // ✅ FIXED: Guest có thể xem nội dung khóa học
        public async Task<
            ActionResult<PaginatedApiResponse<CourseContentResponseDto>>
        > GetCourseContents(
            int courseId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var result = await _courseService.GetCourseContentsAsync(
                    courseId,
                    pageIndex,
                    pageSize
                );
                return Ok(PaginatedApiResponse<CourseContentResponseDto>.SuccessResult(result));
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
        /// Get active contents only (for learning)
        /// PUBLIC: Guest có thể xem nội dung active
        /// </summary>
        [HttpGet("{courseId}/contents/active")]
        [AllowAnonymous] // ✅ FIXED: Guest có thể xem nội dung active
        public async Task<
            ActionResult<ApiResponse<List<CourseContentResponseDto>>>
        > GetActiveCourseContents(int courseId)
        {
            try
            {
                var result = await _courseService.GetActiveContentsAsync(courseId);
                return Ok(ApiResponse<List<CourseContentResponseDto>>.SuccessResult(result));
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
        /// Get specific content details
        /// PUBLIC: Guest có thể xem chi tiết nội dung
        /// </summary>
        [HttpGet("contents/{contentId}")]
        [AllowAnonymous] // ✅ FIXED: Guest có thể xem chi tiết nội dung
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> GetCourseContent(
            int contentId
        )
        {
            try
            {
                var result = await _courseService.GetCourseContentByIdAsync(contentId);
                return Ok(ApiResponse<CourseContentResponseDto>.SuccessResult(result));
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
        /// [TASK] API create CourseContent - Cho phép up khóa học lên trong api
        /// Staff: Quản lý nội dung chương trình
        /// Consultant: Tạo nội dung chuyên môn
        /// </summary>
        [HttpPost("contents")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> CreateCourseContent(
            [FromBody] CreateCourseContentDto createDto
        )
        {
            try
            {
                var result = await _courseService.CreateCourseContentAsync(createDto);
                return CreatedAtAction(
                    nameof(GetCourseContent),
                    new { contentId = result.ContentID },
                    ApiResponse<CourseContentResponseDto>.SuccessResult(
                        result,
                        "Tạo nội dung bài học thành công"
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo nội dung bài học")
                );
            }
        }

        /// <summary>
        /// [TASK] API update CourseContent - Cho Staff, Consultant, Manager, Admin
        /// </summary>
        [HttpPut("contents/{contentId}")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> UpdateCourseContent(
            int contentId,
            [FromBody] UpdateCourseContentDto updateDto
        )
        {
            try
            {
                if (contentId != updateDto.ContentID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _courseService.UpdateCourseContentAsync(updateDto);
                return Ok(
                    ApiResponse<CourseContentResponseDto>.SuccessResult(
                        result,
                        "Cập nhật nội dung bài học thành công"
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi cập nhật nội dung bài học")
                );
            }
        }

        /// <summary>
        /// [TASK] API delete CourseContent - Xóa bằng cách thay đổi trạng thái isActive = false
        /// </summary>
        [HttpDelete("contents/{contentId}")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<string>>> DeleteCourseContent(int contentId)
        {
            try
            {
                await _courseService.DeleteCourseContentAsync(contentId);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa nội dung bài học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi xóa nội dung bài học")
                );
            }
        }

        /// <summary>
        /// Get next order index for new content
        /// </summary>
        [HttpGet("{courseId}/contents/next-order")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<int>>> GetNextOrderIndex(int courseId)
        {
            try
            {
                var result = await _courseService.GetNextOrderIndexAsync(courseId);
                return Ok(ApiResponse<int>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Reorder course contents
        /// </summary>
        [HttpPatch("{courseId}/contents/reorder")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<ActionResult<ApiResponse<string>>> ReorderCourseContents(
            int courseId,
            [FromBody] Dictionary<int, int> contentOrderMapping
        )
        {
            try
            {
                await _courseService.ReorderCourseContentsAsync(courseId, contentOrderMapping);
                return Ok(ApiResponse<string>.SuccessResult("", "Sắp xếp lại thứ tự thành công"));
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

        #endregion

        #region ✅ Course Registration - Member APIs

        /// <summary>
        /// [TASK] API cho phép member đăng ký khóa học - Ghi thông tin ở bảng registrationCourse
        /// Member: Đăng ký khóa học để học
        /// Consultant: Có thể đăng ký để tham khảo
        /// Manager/Admin: Có thể đăng ký để giám sát
        /// </summary>
        [HttpPost("{courseId}/register")]
        [Authorize(Roles = "Member,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Admin
        public async Task<
            ActionResult<ApiResponse<CourseRegistrationResponseDto>>
        > RegisterForCourse(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.RegisterForCourseAsync(courseId, userId);
                return Ok(
                    ApiResponse<CourseRegistrationResponseDto>.SuccessResult(
                        result,
                        "Đăng ký khóa học thành công"
                    )
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi đăng ký khóa học")
                );
            }
        }

        /// <summary>
        /// Unregister from course
        /// </summary>
        [HttpDelete("{courseId}/unregister")]
        [Authorize(Roles = "Member,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Admin
        public async Task<ActionResult<ApiResponse<string>>> UnregisterFromCourse(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _courseService.UnregisterFromCourseAsync(courseId, userId);
                return Ok(ApiResponse<string>.SuccessResult("", "Hủy đăng ký khóa học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Check if user can register for course
        /// Chỉ cần đăng nhập là có thể check
        /// </summary>
        [HttpGet("{courseId}/can-register")]
        [Authorize] // ✅ KEEP: Cần đăng nhập để check đăng ký
        public async Task<ActionResult<ApiResponse<bool>>> CanUserRegister(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.CanUserRegisterAsync(courseId, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Check if user is registered for course
        /// Chỉ cần đăng nhập là có thể check
        /// </summary>
        [HttpGet("{courseId}/is-registered")]
        [Authorize] // ✅ KEEP: Cần đăng nhập để check trạng thái đăng ký
        public async Task<ActionResult<ApiResponse<bool>>> IsUserRegistered(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.IsUserRegisteredAsync(courseId, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Registration Management & Statistics

        /// <summary>
        /// Get registrations for a course (for instructors/managers)
        /// </summary>
        [HttpGet("{courseId}/registrations")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<
            ActionResult<PaginatedApiResponse<RegistrationListDto>>
        > GetCourseRegistrations(int courseId, [FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                var result = await _courseService.GetCourseRegistrationsAsync(courseId, filter);
                return Ok(PaginatedApiResponse<RegistrationListDto>.SuccessResult(result));
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
        /// Get enrollment statistics for a course
        /// </summary>
        [HttpGet("{courseId}/enrollment-stats")]
        [Authorize(Roles = "Staff,Consultant,Manager,Admin")] // ✅ FIXED: Thêm Staff và Admin
        public async Task<
            ActionResult<ApiResponse<CourseEnrollmentStatsDto>>
        > GetCourseEnrollmentStats(int courseId)
        {
            try
            {
                var result = await _courseService.GetCourseEnrollmentStatsAsync(courseId);
                return Ok(ApiResponse<CourseEnrollmentStatsDto>.SuccessResult(result));
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
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

    // ==============================================
    // USER LEARNING CONTROLLER (Additional APIs)
    // ==============================================

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ KEEP: Learning controller cần đăng nhập
    public class LearningController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public LearningController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Get user's learning dashboard
        /// Tất cả role đã đăng nhập đều có thể xem dashboard của mình
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<UserLearningDashboardDto>>> GetUserDashboard()
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.GetUserDashboardAsync(userId);
                return Ok(ApiResponse<UserLearningDashboardDto>.SuccessResult(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Get user's course registrations
        /// Tất cả role đã đăng nhập đều có thể xem khóa học của mình
        /// </summary>
        [HttpGet("my-courses")]
        public async Task<
            ActionResult<PaginatedApiResponse<RegistrationListDto>>
        > GetMyRegistrations([FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.GetUserRegistrationsAsync(userId, filter);
                return Ok(PaginatedApiResponse<RegistrationListDto>.SuccessResult(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Update learning progress
        /// Tất cả role đã đăng nhập đều có thể cập nhật tiến độ học tập
        /// </summary>
        [HttpPatch("progress")]
        public async Task<ActionResult<ApiResponse<CourseRegistrationResponseDto>>> UpdateProgress(
            [FromBody] UpdateProgressDto updateDto
        )
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.UpdateProgressAsync(updateDto, userId);
                return Ok(
                    ApiResponse<CourseRegistrationResponseDto>.SuccessResult(
                        result,
                        "Cập nhật tiến độ thành công"
                    )
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }
    }
}