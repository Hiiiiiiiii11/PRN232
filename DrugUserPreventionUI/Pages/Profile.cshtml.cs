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
            
            // Initialize forms to prevent null reference
            ProfileForm = new ProfileFormModel();
            PasswordForm = new ChangePasswordFormModel();
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
                
                // Ensure form is populated even if user data loading failed
                if (ProfileForm.FullName == null && CurrentUser != null)
                {
                    PopulateProfileForm();
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                
                // Try to populate form with session data as fallback
                TryPopulateFromSession();
                
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            // Debug info
            System.Diagnostics.Debug.WriteLine("=== Profile Update Started ===");
            
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            // Only validate ProfileForm fields, ignore PasswordForm
            var profileErrors = new List<string>();
            var profileFieldPrefix = "ProfileForm.";
            
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith(profileFieldPrefix)))
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Profile Field Error - {key}: {error.ErrorMessage}");
                        profileErrors.Add($"{key.Replace(profileFieldPrefix, "")}: {error.ErrorMessage}");
                    }
                }
            }
            
            if (profileErrors.Any())
            {
                ErrorMessage = $"Thông tin cá nhân không hợp lệ: {string.Join("; ", profileErrors)}";
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Use proper DTO instead of anonymous object
                var updateRequest = new UpdateProfileRequest
                {
                    FullName = ProfileForm.FullName ?? "",
                    Email = ProfileForm.Email ?? "",
                    Phone = ProfileForm.Phone,
                    DateOfBirth = ProfileForm.DateOfBirth,
                    Gender = ProfileForm.Gender
                };

                var userId = GetCurrentUserId();
                var apiUrl = $"https://localhost:7045/api/User/{userId}";
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Update API URL: {apiUrl}");
                System.Diagnostics.Debug.WriteLine($"Update Data: {JsonSerializer.Serialize(updateRequest)}");

                var response = await client.PutAsJsonAsync(apiUrl, updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Update Success: {responseContent}");
                    
                    SuccessMessage = "Cập nhật thông tin cá nhân thành công!";
                    
                    // Clear error message if any
                    ErrorMessage = null;
                    
                    // Reload user data
                    await LoadUserDataAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Update Failed: {response.StatusCode} - {errorContent}");
                    
                    ErrorMessage = $"Lỗi cập nhật ({response.StatusCode}): {errorContent}";
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unauthorized: {ex.Message}");
                ErrorMessage = "Không thể xác định người dùng. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Exception: {ex.Message}");
                ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
            }

            // Always reload data and return to page
            try
            {
                await LoadUserDataAsync();
                await LoadUserStatsAsync();
                await LoadRecentActivitiesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reloading data: {ex.Message}");
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Login", new { returnUrl = "/Profile" });
            }

            // Only validate PasswordForm fields, ignore ProfileForm
            var passwordErrors = new List<string>();
            var passwordFieldPrefix = "PasswordForm.";
            
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith(passwordFieldPrefix)))
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Password Field Error - {key}: {error.ErrorMessage}");
                        passwordErrors.Add($"{key.Replace(passwordFieldPrefix, "")}: {error.ErrorMessage}");
                    }
                }
            }
            
            if (passwordErrors.Any())
            {
                ErrorMessage = $"Thông tin mật khẩu không hợp lệ: {string.Join("; ", passwordErrors)}";
                await LoadUserDataAsync();
                return Page();
            }

            if (PasswordForm.NewPassword != PasswordForm.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp";
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
                    ErrorMessage = "Không tìm thấy token xác thực. Vui lòng đăng nhập lại.";
                    return;
                }

                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var userId = GetCurrentUserId();
                var apiUrl = $"https://localhost:7045/api/User/{userId}";
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Profile API Call: {apiUrl}");
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
                
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Try different response types
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse<UserDTO>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (apiResponse?.Success == true && apiResponse.Data != null)
                        {
                            CurrentUser = apiResponse.Data;
                            PopulateProfileForm();
                        }
                        else
                        {
                            // Try alternate response format
                            var altResponse = JsonSerializer.Deserialize<ApiResponse<UserDTO>>(content, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (altResponse?.Success == true && altResponse.Data != null)
                            {
                                CurrentUser = altResponse.Data;
                                PopulateProfileForm();
                            }
                            else
                            {
                                ErrorMessage = "Không thể tải thông tin người dùng từ server.";
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Try direct deserialization
                        var userData = JsonSerializer.Deserialize<UserDTO>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (userData != null)
                        {
                            CurrentUser = userData;
                            PopulateProfileForm();
                        }
                        else
                        {
                            ErrorMessage = "Lỗi khi xử lý dữ liệu người dùng.";
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể tải thông tin người dùng. Mã lỗi: {response.StatusCode}. Chi tiết: {errorContent}";
                    
                    // Debug info
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error Content: {errorContent}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
            }
        }

        private void PopulateProfileForm()
        {
            if (CurrentUser != null)
            {
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

        private void TryPopulateFromSession()
        {
            try
            {
                // First try to repopulate session from JWT token if session is empty
                TryRepopulateSessionFromToken();
                
                // Try to get basic info from session
                var fullName = HttpContext.Session.GetString("user_name");
                var email = HttpContext.Session.GetString("user_email");
                var role = HttpContext.Session.GetString("user_role");

                if (!string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(email))
                {
                    ProfileForm = new ProfileFormModel
                    {
                        FullName = fullName ?? "",
                        Email = email ?? "",
                        Phone = ProfileForm.Phone, // Keep existing values
                        DateOfBirth = ProfileForm.DateOfBirth,
                        Gender = ProfileForm.Gender
                    };

                    // Create basic CurrentUser from session if not already set
                    if (CurrentUser == null)
                    {
                        CurrentUser = new UserDTO
                        {
                            FullName = fullName,
                            Email = email,
                            Role = role,
                            Username = email // Use email as username fallback
                        };
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors in fallback method
            }
        }

        private void TryRepopulateSessionFromToken()
        {
            try
            {
                var token = HttpContext.Request.Cookies["auth_token"];
                if (string.IsNullOrEmpty(token)) return;

                // Check if session already has user_id
                var existingUserId = HttpContext.Session.GetString("user_id");
                if (!string.IsNullOrEmpty(existingUserId)) return;

                // Decode JWT token to get user info
                var userInfo = DecodeJwtToken(token);
                if (userInfo != null)
                {
                    HttpContext.Session.SetString("user_id", userInfo.UserId.ToString());
                    HttpContext.Session.SetString("user_name", userInfo.UserName);
                    HttpContext.Session.SetString("user_email", userInfo.Email);
                    HttpContext.Session.SetString("user_role", userInfo.Role);
                    
                    System.Diagnostics.Debug.WriteLine("Session repopulated from JWT token");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error repopulating session: {ex.Message}");
            }
        }

        // JWT token decoder (simplified version)
        private JwtUserInfo? DecodeJwtToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1];
                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new JwtUserInfo
                {
                    UserId = root.TryGetProperty("nameid", out var userIdProp) ? 
                        (int.TryParse(userIdProp.GetString(), out var uid) ? uid : 0) : 0,
                    UserName = root.TryGetProperty("unique_name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                    Email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? "" : "",
                    Role = root.TryGetProperty("role", out var roleProp) ? roleProp.GetString() ?? "" : ""
                };
            }
            catch
            {
                return null;
            }
        }

        // Helper class for JWT token data
        private class JwtUserInfo
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
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
            // Debug info
            System.Diagnostics.Debug.WriteLine("=== Getting Current User ID ===");
            
            // Try to repopulate session from token if needed
            TryRepopulateSessionFromToken();
            
            // Try to get user ID from session first
            var userIdFromSession = HttpContext.Session.GetString("user_id");
            System.Diagnostics.Debug.WriteLine($"Session user_id: {userIdFromSession}");
            
            if (!string.IsNullOrEmpty(userIdFromSession) && int.TryParse(userIdFromSession, out int sessionUserId))
            {
                System.Diagnostics.Debug.WriteLine($"Using session user ID: {sessionUserId}");
                return sessionUserId;
            }

            // Fallback to JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            System.Diagnostics.Debug.WriteLine($"JWT NameIdentifier claim: {userIdClaim?.Value}");
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                System.Diagnostics.Debug.WriteLine($"Using JWT user ID: {userId}");
                return userId;
            }

            // Try alternative session keys
            var altSessionKeys = new[] { "UserId", "user_Id", "UserID" };
            foreach (var key in altSessionKeys)
            {
                var altUserId = HttpContext.Session.GetString(key);
                System.Diagnostics.Debug.WriteLine($"Session {key}: {altUserId}");
                if (!string.IsNullOrEmpty(altUserId) && int.TryParse(altUserId, out int altId))
                {
                    System.Diagnostics.Debug.WriteLine($"Using alternative session user ID from {key}: {altId}");
                    return altId;
                }
            }
            
            // List all session keys for debugging
            System.Diagnostics.Debug.WriteLine("=== All Session Keys ===");
            foreach (var key in HttpContext.Session.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"Session Key: {key} = {HttpContext.Session.GetString(key)}");
            }
            
            throw new UnauthorizedAccessException($"Không thể xác định người dùng hiện tại. Session user_id: {userIdFromSession}, JWT claim: {userIdClaim?.Value}");
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

        [RegularExpression(@"^[0-9+\-\s\(\)]{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ (10-15 chữ số)")]
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