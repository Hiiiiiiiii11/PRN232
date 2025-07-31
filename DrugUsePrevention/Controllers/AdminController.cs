using BussinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Admin;
using Services.IService;
using System.Security.Claims;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            try
            {
                var result = await _adminService.GetUserByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userResponse = MapToUserResponse(result);
                return Ok(new { message = "Success", data = userResponse });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail([FromRoute] string email)
        {
            try
            {
                var result = await _adminService.GetUserByEmailAsync(email);
                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userResponse = MapToUserResponse(result);
                return Ok(new { message = "Success", data = userResponse });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            try
            {
                var result = await _adminService.GetUserByUsernameAsync(username);
                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userResponse = MapToUserResponse(result);
                return Ok(new { message = "Success", data = userResponse });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync();
                var userResponses = result.Select(MapToUserResponse).ToList();
                return Ok(new { message = "Success", data = userResponses });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetUsersByStatus([FromRoute] string status)
        {
            try
            {
                var result = await _adminService.GetUsersByStatusAsync(status);
                var userResponses = result.Select(MapToUserResponse).ToList();
                return Ok(new { message = "Success", data = userResponses });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole([FromRoute] string role)
        {
            try
            {
                var result = await _adminService.GetUsersByRoleAsync(role);
                var userResponses = result.Select(MapToUserResponse).ToList();
                return Ok(new { message = "Success", data = userResponses });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.Name))
                {
                    var searchResult = await _adminService.SearchUsersAsync(request.Name);
                    var userResponses = searchResult.Select(MapToUserResponse).ToList();
                    return Ok(new { message = "Success", data = userResponses });
                }

                var paginationResult = await _adminService.GetUsersWithPaginationAsync(request.Page, request.PageSize);
                var response = new PaginatedUsersResponse
                {
                    Users = paginationResult.Users.Select(MapToUserResponse).ToList(),
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Validation failed", errors = ModelState });
                }

                var user = new User
                {
                    FullName = request.FullName,
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = request.Password, // Will be hashed in service
                    Phone = request.Phone,
                    AvatarUrl = request.AvatarUrl,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Role = request.Role
                };

                var result = await _adminService.CreateUserAsync(user);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Validation failed", errors = ModelState });
                }

                var user = new User
                {
                    UserID = request.UserID,
                    FullName = request.FullName,
                    Username = request.Username,
                    Email = request.Email,
                    Phone = request.Phone,
                    AvatarUrl = request.AvatarUrl,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Role = request.Role,
                    Status = request.Status,
                    IsEmailVerified = request.IsEmailVerified
                };

                var result = await _adminService.UpdateUserAsync(user);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(id);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("ban/{id}")]
        public async Task<IActionResult> BanUser([FromRoute] int id)
        {
            try
            {
                var result = await _adminService.BanUserAsync(id);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("unban/{id}")]
        public async Task<IActionResult> UnbanUser([FromRoute] int id)
        {
            try
            {
                var result = await _adminService.UnbanUserAsync(id);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Validation failed", errors = ModelState });
                }

                var result = await _adminService.ChangeUserRoleAsync(request.UserID, request.NewRole);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("verify-email/{id}")]
        public async Task<IActionResult> VerifyUserEmail([FromRoute] int id)
        {
            try
            {
                var result = await _adminService.VerifyUserEmailAsync(id);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetUserPassword([FromBody] ResetPasswordRequest request)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { message = "Validation failed", errors = ModelState });
        //        }

        //        var result = await _adminService.ResetUserPasswordAsync(request.UserID, request.NewPassword);
        //        if (!result.Success)
        //        {
        //            return BadRequest(new { message = result.Message });
        //        }

        //        return Ok(new { message = result.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return StatusCode(500, new { message = "An error occurred" });
        //    }
        //}

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] AdminChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Validation failed", errors = ModelState });
                }

                var result = await _adminService.ChangeUserPasswordAsync(request.UserID, request.OldPassword, request.NewPassword);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var totalUsers = await _adminService.GetTotalUsersCountAsync();
                var usersByRole = await _adminService.GetUsersByRoleStatisticsAsync();
                var usersByStatus = await _adminService.GetUsersByStatusStatisticsAsync();
                var recentUsers = await _adminService.GetRecentlyRegisteredUsersAsync(5);

                var response = new UserStatisticsResponse
                {
                    TotalUsers = totalUsers,
                    UsersByRole = usersByRole,
                    UsersByStatus = usersByStatus,
                    RecentUsers = recentUsers.Select(MapToUserResponse).ToList()
                };

                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmailAvailability([FromRoute] string email)
        {
            try
            {
                var isAvailable = await _adminService.IsEmailAvailableAsync(email);
                return Ok(new { message = "Success", data = new { isAvailable } });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsernameAvailability([FromRoute] string username)
        {
            try
            {
                var isAvailable = await _adminService.IsUsernameAvailableAsync(username);
                return Ok(new { message = "Success", data = new { isAvailable } });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Private helper method to map User entity to UserResponse DTO
        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                IsEmailVerified = user.IsEmailVerified
            };
        }
    }
}