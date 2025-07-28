using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using DrugUserPreventionUI.Models.Users;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Helper;

namespace DrugUserPreventionUI.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        [BindProperty]
        public ProfileFormModel ProfileForm { get; set; } = new();

        [BindProperty]
        public ChangePasswordFormModel PasswordForm { get; set; } = new();

        public UserDTO? CurrentUser { get; set; }
        public UserStatsModel? UserStats { get; set; }
        public List<ActivityModel>? RecentActivities { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            try
            {
                await LoadUserDataAsync();
                await LoadUserStatsAsync();
                await LoadRecentActivitiesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updateRequest = new
                {
                    FullName = ProfileForm.FullName,
                    Email = ProfileForm.Email,
                    Phone = ProfileForm.Phone,
                    DateOfBirth = ProfileForm.DateOfBirth,
                    Gender = ProfileForm.Gender
                };

                var userId = GetCurrentUserId();
                var response = await client.PutAsJsonAsync($"https://localhost:7045/api/User/{userId}", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cập nhật thông tin cá nhân thành công!";
                    await LoadUserDataAsync(); // Reload user data
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = "Có lỗi xảy ra khi cập nhật thông tin: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            await LoadUserDataAsync();
            await LoadUserStatsAsync();
            await LoadRecentActivitiesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            if (PasswordForm.NewPassword != PasswordForm.ConfirmPassword)
            {
                ModelState.AddModelError("PasswordForm.ConfirmPassword", "Mật khẩu xác nhận không khớp");
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var changePasswordRequest = new
                {
                    CurrentPassword = PasswordForm.CurrentPassword,
                    NewPassword = PasswordForm.NewPassword
                };

                var userId = GetCurrentUserId();
                var response = await client.PostAsJsonAsync($"https://localhost:7045/api/User/{userId}/change-password", changePasswordRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đổi mật khẩu thành công!";
                    PasswordForm = new ChangePasswordFormModel(); // Clear form
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = "Có lỗi xảy ra khi đổi mật khẩu: " + (errorContent.Contains("Invalid") ? "Mật khẩu hiện tại không đúng" : errorContent);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            await LoadUserDataAsync();
            await LoadUserStatsAsync();
            await LoadRecentActivitiesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile AvatarFile)
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                ErrorMessage = "Vui lòng chọn file ảnh";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(AvatarFile.ContentType.ToLower()))
            {
                ErrorMessage = "Chỉ chấp nhận file ảnh định dạng JPG, JPEG, PNG";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file size (2MB)
            if (AvatarFile.Length > 2 * 1024 * 1024)
            {
                ErrorMessage = "Kích thước file không được vượt quá 2MB";
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{GetCurrentUserId()}_{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                // Update user avatar URL
                var avatarUrl = $"/uploads/avatars/{fileName}";
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updateRequest = new { AvatarUrl = avatarUrl };
                var userId = GetCurrentUserId();
                var response = await client.PutAsJsonAsync($"https://localhost:7045/api/User/{userId}/avatar", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cập nhật ảnh đại diện thành công!";
                    await LoadUserDataAsync(); // Reload user data
                }
                else
                {
                    // Delete uploaded file if API call failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    ErrorMessage = "Có lỗi xảy ra khi cập nhật ảnh đại diện";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra khi tải lên ảnh: " + ex.Message;
            }

            await LoadUserDataAsync();
            await LoadUserStatsAsync();
            await LoadRecentActivitiesAsync();
            return Page();
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = HttpContext.Request.Cookies["auth_token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    return;
                }

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var userId = GetCurrentUserId();
                var response = await client.GetAsync($"https://localhost:7045/api/User/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse<UserDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        CurrentUser = apiResponse.Data;
                        
                        // Populate form with current user data
                        ProfileForm = new ProfileFormModel
                        {
                            FullName = CurrentUser.FullName ?? "",
                            Email = CurrentUser.Email ?? "",
                            Phone = CurrentUser.Phone,
                            DateOfBirth = CurrentUser.DateOfBirth,
                            Gender = CurrentUser.Gender
                        };
                    }
                }
            }
            catch (Exception)
            {
                // Handle error silently or log
            }
        }

        private async Task LoadUserStatsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = HttpContext.Request.Cookies["auth_token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    return;
                }

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var userId = GetCurrentUserId();
                
                // Load course stats
                var courseResponse = await client.GetAsync($"https://localhost:7045/api/Course/user/{userId}/stats");
                var appointmentResponse = await client.GetAsync($"https://localhost:7045/api/Appointment/user/{userId}/stats");

                UserStats = new UserStatsModel
                {
                    CompletedCourses = 0,
                    TotalAppointments = 0
                };

                if (courseResponse.IsSuccessStatusCode)
                {
                    var courseContent = await courseResponse.Content.ReadAsStringAsync();
                    // Parse course stats if API exists
                }

                if (appointmentResponse.IsSuccessStatusCode)
                {
                    var appointmentContent = await appointmentResponse.Content.ReadAsStringAsync();
                    // Parse appointment stats if API exists
                }
            }
            catch (Exception)
            {
                UserStats = new UserStatsModel { CompletedCourses = 0, TotalAppointments = 0 };
            }
        }

        private async Task LoadRecentActivitiesAsync()
        {
            try
            {
                // Mock data for now - replace with actual API call
                RecentActivities = new List<ActivityModel>
                {
                    new ActivityModel
                    {
                        Title = "Hoàn thành khóa học",
                        Description = "Bạn đã hoàn thành khóa học 'Phòng chống tệ nạn xã hội'",
                        Type = "success",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    new ActivityModel
                    {
                        Title = "Đặt lịch tư vấn",
                        Description = "Bạn đã đặt lịch tư vấn với chuyên gia Dr. Nguyễn Văn A",
                        Type = "info",
                        CreatedAt = DateTime.Now.AddDays(-3)
                    },
                    new ActivityModel
                    {
                        Title = "Cập nhật hồ sơ",
                        Description = "Bạn đã cập nhật thông tin cá nhân",
                        Type = "warning",
                        CreatedAt = DateTime.Now.AddDays(-7)
                    }
                };
            }
            catch (Exception)
            {
                RecentActivities = new List<ActivityModel>();
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

        public string GetRoleBadgeClass()
        {
            return CurrentUser?.Role?.ToLower() switch
            {
                "admin" => "danger",
                "manager" => "purple",
                "staff" => "warning",
                "consultant" => "info",
                "member" => "success",
                _ => "secondary"
            };
        }

        public string GetRoleIcon()
        {
            return CurrentUser?.Role?.ToLower() switch
            {
                "admin" => "crown",
                "manager" => "user-tie",
                "staff" => "user-cog",
                "consultant" => "user-md",
                "member" => "user",
                _ => "user"
            };
        }

        public string GetMembershipDuration()
        {
            if (CurrentUser?.CreatedAt == null) return "0 ngày";
            
            var duration = DateTime.Now - CurrentUser.CreatedAt;
            if (duration.TotalDays < 1) return "Hôm nay";
            if (duration.TotalDays < 30) return $"{(int)duration.TotalDays} ngày";
            if (duration.TotalDays < 365) return $"{(int)(duration.TotalDays / 30)} tháng";
            return $"{(int)(duration.TotalDays / 365)} năm";
        }
    }

    public class ProfileFormModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }
    }

    public class ChangePasswordFormModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class UserStatsModel
    {
        public int CompletedCourses { get; set; }
        public int TotalAppointments { get; set; }
    }

    public class ActivityModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "info"; // success, info, warning, danger
        public DateTime CreatedAt { get; set; }
    }
} 