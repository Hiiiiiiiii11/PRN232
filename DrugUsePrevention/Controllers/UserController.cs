using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BussinessObjects;
using Services.DTOs.User;
using Services.DTOs.Common;
using Repositories.IRepository.Users;

namespace DrugUsePrevention.Controllers
{
    // Temporarily disabled to fix Swagger
    /*
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetUser(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only access their own profile unless they're admin
                if (currentUserId != id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy người dùng"));
                }

                var userDto = new UserDTO
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    DateOfBirth = user.DateOfBirth ?? DateTime.MinValue,
                    Gender = user.Gender,
                    IsEmailVerified = user.IsEmailVerified
                };

                return Ok(ApiResponse<UserDTO>.SuccessResult(userDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only update their own profile unless they're admin
                if (currentUserId != id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy người dùng"));
                }

                // Update user properties
                user.FullName = request.FullName;
                user.Email = request.Email;
                user.Phone = request.Phone;
                user.DateOfBirth = request.DateOfBirth;
                user.Gender = request.Gender;

                await _userRepository.UpdateAsync(user);

                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật thông tin thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword(int id, [FromBody] Services.DTOs.User.ChangePasswordRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only change their own password
                if (currentUserId != id)
                {
                    return Forbid();
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy người dùng"));
                }

                // Verify current password
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Mật khẩu hiện tại không đúng"));
                }

                // Hash new password
                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                await _userRepository.UpdateAsync(user);

                return Ok(ApiResponse<string>.SuccessResult("", "Đổi mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Update user avatar
        /// </summary>
        [HttpPut("{id}/avatar")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateAvatar(int id, [FromBody] UpdateAvatarRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only update their own avatar unless they're admin
                if (currentUserId != id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy người dùng"));
                }

                user.AvatarUrl = request.AvatarUrl;
                await _userRepository.UpdateAsync(user);

                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật ảnh đại diện thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<UserStatsResponse>>> GetUserStats(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Users can only access their own stats unless they're admin
                if (currentUserId != id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Không tìm thấy người dùng"));
                }

                // Calculate user statistics
                var stats = new UserStatsResponse
                {
                    CompletedCourses = user.CourseRegistrations?.Count(r => r.Completed) ?? 0,
                    TotalCourses = user.CourseRegistrations?.Count ?? 0,
                    TotalAppointments = user.Appointments?.Count ?? 0,
                    MemberSince = user.CreatedAt
                };

                return Ok(ApiResponse<UserStatsResponse>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }
    }

    // DTOs for User Controller
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    public class UpdateAvatarRequest
    {
        public string AvatarUrl { get; set; } = "";
    }

    public class UserStatsResponse
    {
        public int CompletedCourses { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAppointments { get; set; }
        public DateTime MemberSince { get; set; }
    }
    */
} 