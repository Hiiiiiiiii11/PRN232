using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Common;
using Services.DTOs.Consultant;
using Services.IService;
using System.Security.Claims;

namespace DrugUsePrevention.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultantCalendarController : ControllerBase
    {
        private readonly IConsultantCalendarService _calendarService;

        public ConsultantCalendarController(IConsultantCalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        #region Schedule Management

        /// <summary>
        /// Get consultant's weekly schedule
        /// </summary>
        [HttpGet("{consultantId}/schedule")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<ConsultantScheduleDto>>>> GetConsultantSchedule(int consultantId)
        {
            try
            {
                var result = await _calendarService.GetConsultantScheduleAsync(consultantId);
                return Ok(ApiResponse<List<ConsultantScheduleDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Create schedule for consultant
        /// </summary>
        [HttpPost("{consultantId}/schedule")]
        [Authorize(Roles = "Consultant,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<ConsultantScheduleDto>>> CreateSchedule(
            int consultantId, 
            [FromBody] CreateConsultantScheduleDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only the consultant themselves, or admin/manager can create schedules
                if (userRole != "Admin" && userRole != "Manager" && currentUserId != consultantId)
                {
                    return Forbid("Bạn không có quyền tạo lịch trình cho consultant này.");
                }

                var result = await _calendarService.CreateScheduleAsync(consultantId, createDto);
                return CreatedAtAction(nameof(GetConsultantSchedule), 
                    new { consultantId }, 
                    ApiResponse<ConsultantScheduleDto>.SuccessResult(result, "Tạo lịch trình thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Update consultant schedule
        /// </summary>
        [HttpPut("{consultantId}/schedule")]
        [Authorize(Roles = "Consultant,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<ConsultantScheduleDto>>> UpdateSchedule(
            int consultantId, 
            [FromBody] UpdateConsultantScheduleDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (userRole != "Admin" && userRole != "Manager" && currentUserId != consultantId)
                {
                    return Forbid("Bạn không có quyền cập nhật lịch trình cho consultant này.");
                }

                var result = await _calendarService.UpdateScheduleAsync(consultantId, updateDto);
                return Ok(ApiResponse<ConsultantScheduleDto>.SuccessResult(result, "Cập nhật lịch trình thành công"));
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
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Delete consultant schedule
        /// </summary>
        [HttpDelete("{consultantId}/schedule/{scheduleId}")]
        [Authorize(Roles = "Consultant,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSchedule(int consultantId, int scheduleId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (userRole != "Admin" && userRole != "Manager" && currentUserId != consultantId)
                {
                    return Forbid("Bạn không có quyền xóa lịch trình cho consultant này.");
                }

                var success = await _calendarService.DeleteScheduleAsync(consultantId, scheduleId);
                if (!success)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy lịch trình"));
                }

                return Ok(ApiResponse<string>.SuccessResult("", "Xóa lịch trình thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Calendar Views

        /// <summary>
        /// Get weekly calendar for consultant
        /// </summary>
        [HttpGet("{consultantId}/calendar/weekly")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetWeeklyCalendar(
            int consultantId, 
            [FromQuery] DateTime? weekStartDate = null)
        {
            try
            {
                var startDate = weekStartDate ?? GetStartOfWeek(DateTime.Today);
                var result = await _calendarService.GetWeeklyCalendarAsync(consultantId, startDate);
                return Ok(ApiResponse<WeeklyCalendarDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get daily calendar for consultant
        /// </summary>
        [HttpGet("{consultantId}/calendar/daily")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ConsultantCalendarDto>>> GetDailyCalendar(
            int consultantId, 
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                var result = await _calendarService.GetDailyCalendarAsync(consultantId, targetDate);
                return Ok(ApiResponse<ConsultantCalendarDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get available slots for a specific date
        /// </summary>
        [HttpGet("{consultantId}/available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<AppointmentSlotDto>>>> GetAvailableSlots(
            int consultantId, 
            [FromQuery] DateTime date)
        {
            try
            {
                var result = await _calendarService.GetAvailableSlotsAsync(consultantId, date);
                return Ok(ApiResponse<List<AppointmentSlotDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get next available slots
        /// </summary>
        [HttpGet("{consultantId}/next-available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<DateTime>>>> GetNextAvailableSlots(
            int consultantId, 
            [FromQuery] int count = 10)
        {
            try
            {
                var result = await _calendarService.GetNextAvailableSlotsAsync(consultantId, count);
                return Ok(ApiResponse<List<DateTime>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Availability Management

        /// <summary>
        /// Get consultant availability for date range
        /// </summary>
        [HttpGet("{consultantId}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<ConsultantAvailabilityDto>>>> GetAvailability(
            int consultantId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _calendarService.GetConsultantAvailabilityAsync(consultantId, startDate, endDate);
                return Ok(ApiResponse<List<ConsultantAvailabilityDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Set availability exception
        /// </summary>
        [HttpPost("{consultantId}/availability")]
        [Authorize(Roles = "Consultant,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<ConsultantAvailabilityDto>>> SetAvailability(
            int consultantId,
            [FromBody] CreateConsultantAvailabilityDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (userRole != "Admin" && userRole != "Manager" && currentUserId != consultantId)
                {
                    return Forbid("Bạn không có quyền cập nhật khả năng cho consultant này.");
                }

                var result = await _calendarService.SetAvailabilityAsync(consultantId, createDto);
                return CreatedAtAction(nameof(GetAvailability),
                    new { consultantId, startDate = createDto.Date, endDate = createDto.Date },
                    ApiResponse<ConsultantAvailabilityDto>.SuccessResult(result, "Đã cập nhật khả năng"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// Generate slots for a period
        /// </summary>
        [HttpPost("{consultantId}/generate-slots")]
        [Authorize(Roles = "Consultant,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateSlots(
            int consultantId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (userRole != "Admin" && userRole != "Manager" && currentUserId != consultantId)
                {
                    return Forbid("Bạn không có quyền tạo slot cho consultant này.");
                }

                await _calendarService.GenerateSlotsForPeriodAsync(consultantId, startDate, endDate);
                return Ok(ApiResponse<string>.SuccessResult("", "Đã tạo các slot thành công"));
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

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            return date.AddDays(-dayOfWeek).Date;
        }

        #endregion
    }
}