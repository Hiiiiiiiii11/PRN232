// Pages/ManagerDashboard/ManagerDashboard.cshtml.cs
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages.ManagerDashboard
{
    public class ManagerDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiConfiguration _apiConfig;

        public ManagerDashboardModel(IHttpClientFactory httpClientFactory, ApiConfiguration apiConfig)
        {
            _httpClientFactory = httpClientFactory;
            _apiConfig = apiConfig;
        }

        public List<UserResponse> Consultants { get; set; } = new List<UserResponse>();
        public List<CourseListDto> PendingCourses { get; set; } = new List<CourseListDto>();
        public UserResponse? ConsultantDetail { get; set; }
        public CourseListDto? CourseDetail { get; set; }
        public UserStatisticsResponse? UserStatistics { get; set; }
        public SimpleCourseStats? CourseStatistics { get; set; }
        public int? TotalRegistrations { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        [BindProperty]
        public ProfileUpdateForm ProfileForm { get; set; } = new ProfileUpdateForm();

        [BindProperty]
        public ChangePasswordForm PasswordForm { get; set; } = new ChangePasswordForm();

        // Helper methods to get user info from JWT token
        private LoginModel GetLoginModel()
        {
            var loginModel = new LoginModel(_httpClientFactory);
            loginModel.PageContext = PageContext;
            return loginModel;
        }

        private UserInfoDto? GetCurrentUser()
        {
            return GetLoginModel().GetCurrentUser();
        }

        private bool IsAuthenticated()
        {
            return GetLoginModel().IsAuthenticated();
        }

        private string GetUserRole()
        {
            return GetLoginModel().GetUserRole();
        }

        private string GetDisplayName()
        {
            return GetLoginModel().GetDisplayName();
        }

        private int GetCurrentUserId()
        {
            var user = GetCurrentUser();
            return user?.UserId ?? 0;
        }

        public async Task<IActionResult> OnGetAsync(
            string? action = null,
            int? id = null,
            string? message = null,
            string? messageType = null
        )
        {
            // Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new
                    {
                        message = "Vui lòng đăng nhập để truy cập trang này.",
                        messageType = "warning",
                    }
                );
            }

            // Check if user is Manager
            var userRole = GetUserRole();
            if (userRole != "Manager")
            {
                return RedirectToPage(
                    "/CourseDashboard",
                    new
                    {
                        message = "Bạn không có quyền truy cập trang này.",
                        messageType = "error",
                    }
                );
            }

            CurrentAction = action?.ToLower();

            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            try
            {
                var client = GetAuthenticatedClient();

                // Load dashboard data
                await LoadDashboardData(client);

                switch (CurrentAction)
                {
                    case "consultants":
                        await LoadConsultants(client);
                        break;
                    case "consultant-detail":
                        if (id.HasValue)
                        {
                            await LoadConsultantDetail(client, id.Value);
                        }
                        break;
                    case "pending-courses":
                        await LoadPendingCourses(client);
                        break;
                    case "course-detail":
                        if (id.HasValue)
                        {
                            await LoadCourseDetail(client, id.Value);
                        }
                        break;
                    case "profile":
                        await LoadCurrentUserProfile(client);
                        break;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveCourseAsync(int id, bool isAccept)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Manager")
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new { message = "Bạn không có quyền duyệt khóa học.", messageType = "error" }
                );
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isAccept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{_apiConfig.CoursesApiUrl}/{id}/approve", content);

                if (response.IsSuccessStatusCode)
                {
                    var approvalText = isAccept ? "duyệt" : "từ chối";
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "pending-courses",
                            message = $"Đã {approvalText} khóa học thành công!",
                            messageType = "success",
                        }
                    );
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage(
                        "/Login",
                        new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "pending-courses",
                            message = $"Lỗi khi duyệt khóa học: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new
                    {
                        action = "pending-courses",
                        message = $"Lỗi: {ex.Message}",
                        messageType = "error",
                    }
                );
            }
        }

        public async Task<IActionResult> OnPostBanConsultantAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Manager")
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new { message = "Bạn không có quyền cấm tài khoản.", messageType = "error" }
                );
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{_apiConfig.AdminApiUrl}/ban/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "consultants",
                            message = "Đã cấm tài khoản Consultant thành công!",
                            messageType = "success",
                        }
                    );
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage(
                        "/Login",
                        new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "consultants",
                            message = $"Lỗi khi cấm tài khoản: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new
                    {
                        action = "consultants",
                        message = $"Lỗi: {ex.Message}",
                        messageType = "error",
                    }
                );
            }
        }

        public async Task<IActionResult> OnPostUnbanConsultantAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Manager")
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new { message = "Bạn không có quyền bỏ cấm tài khoản.", messageType = "error" }
                );
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{_apiConfig.AdminApiUrl}/unban/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "consultants",
                            message = "Đã bỏ cấm tài khoản Consultant thành công!",
                            messageType = "success",
                        }
                    );
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage(
                        "/Login",
                        new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "consultants",
                            message = $"Lỗi khi bỏ cấm tài khoản: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/ManagerDashboard",
                    new
                    {
                        action = "consultants",
                        message = $"Lỗi: {ex.Message}",
                        messageType = "error",
                    }
                );
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadDashboardData(GetAuthenticatedClient());
                CurrentAction = "profile";
                Message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var currentUserId = GetCurrentUserId();

                var updateRequest = new UpdateUserRequest
                {
                    UserID = currentUserId,
                    FullName = ProfileForm.FullName,
                    Username = ProfileForm.Email, // Use email as username
                    Email = ProfileForm.Email,
                    Phone = ProfileForm.Phone,
                    Role = "Manager", // Keep current role
                    Status = "Active", // Keep active
                    IsEmailVerified =
                        true // Assume verified
                    ,
                };

                var json = JsonSerializer.Serialize(
                    updateRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{_apiConfig.AdminApiUrl}/update", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "profile",
                            message = "Cập nhật thông tin thành công!",
                            messageType = "success",
                        }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi cập nhật: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadDashboardData(GetAuthenticatedClient());
            CurrentAction = "profile";
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadDashboardData(GetAuthenticatedClient());
                CurrentAction = "profile";
                Message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var currentUserId = GetCurrentUserId();

                var changePasswordRequest = new ChangePasswordRequest
                {
                    UserID = currentUserId,
                    OldPassword = PasswordForm.OldPassword,
                    NewPassword = PasswordForm.NewPassword,
                };

                var json = JsonSerializer.Serialize(
                    changePasswordRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiConfig.AdminApiUrl}/change-password", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/ManagerDashboard",
                        new
                        {
                            action = "profile",
                            message = "Đổi mật khẩu thành công!",
                            messageType = "success",
                        }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi đổi mật khẩu: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadDashboardData(GetAuthenticatedClient());
            CurrentAction = "profile";
            return Page();
        }

        // Helper methods
        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Request.Cookies["auth_token"];

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private async Task LoadDashboardData(HttpClient client)
        {
            try
            {
                // Load user statistics
                await LoadUserStatistics(client);

                // Load course statistics
                await LoadCourseStatistics(client);

                // Load pending courses (top 10)
                await LoadPendingCourses(client, 10);

                // Load consultants (top 10)
                await LoadConsultants(client, 10);

                // Load total registrations
                await LoadTotalRegistrations(client);
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu dashboard: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadUserStatistics(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/statistics");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserStatisticsResponse>
                    >();
                    UserStatistics = apiResponse?.Data;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the flow
                Console.WriteLine($"Error loading user statistics: {ex.Message}");
            }
        }

        private async Task LoadCourseStatistics(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.CoursesApiUrl}/statistics");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<SimpleCourseStats>
                    >();
                    CourseStatistics = apiResponse?.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading course statistics: {ex.Message}");
            }
        }

        private async Task LoadPendingCourses(HttpClient client, int? maxItems = null)
        {
            try
            {
                // Get courses with isAccept = false
                var queryString = $"?isAccept=false&pageIndex=1&pageSize={maxItems ?? 50}";

                var response = await client.GetAsync($"{_apiConfig.CoursesApiUrl}{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        PaginatedApiResponse<CourseListDto>
                    >();
                    PendingCourses = apiResponse?.Data?.ToList() ?? new List<CourseListDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading pending courses: {ex.Message}");
            }
        }

        private async Task LoadConsultants(HttpClient client, int? maxItems = null)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/role/Consultant");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<List<UserResponse>>
                    >();
                    var allConsultants = apiResponse?.Data ?? new List<UserResponse>();

                    Consultants = maxItems.HasValue
                        ? allConsultants.Take(maxItems.Value).ToList()
                        : allConsultants;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading consultants: {ex.Message}");
            }
        }

        private async Task LoadConsultantDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserResponse>
                    >();
                    ConsultantDetail = apiResponse?.Data;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin Consultant: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCourseDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.CoursesApiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<CourseResponseDto>
                    >();
                    if (apiResponse?.Data != null)
                    {
                        var course = apiResponse.Data;
                        CourseDetail = new CourseListDto
                        {
                            CourseID = course.CourseID,
                            Title = course.Title,
                            Description = course.Description,
                            TargetGroup = course.TargetGroup,
                            AgeGroup = course.AgeGroup,
                            CreatorName = course.CreatorName,
                            CreatedAt = course.CreatedAt,
                            IsActive = course.IsActive,
                            IsAccept = course.IsAccept,
                            ThumbnailURL = course.ThumbnailURL,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin khóa học: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCurrentUserProfile(HttpClient client)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/{currentUserId}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserResponse>
                    >();
                    if (apiResponse?.Data != null)
                    {
                        var user = apiResponse.Data;
                        ProfileForm = new ProfileUpdateForm
                        {
                            FullName = user.FullName,
                            Email = user.Email,
                            Phone = user.Phone,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin profile: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadTotalRegistrations(HttpClient client)
        {
            try
            {
                // This would need a specific endpoint for registration statistics
                // For now, we'll use a placeholder
                TotalRegistrations = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading total registrations: {ex.Message}");
            }
        }
    }

    // DTOs for Manager Dashboard
    public class ProfileUpdateForm
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }
    }

    public class ChangePasswordForm
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }

    // Reuse DTOs from other dashboards
    public class UserResponse
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class UserStatisticsResponse
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UsersByStatus { get; set; } = new Dictionary<string, int>();
        public List<UserResponse> RecentUsers { get; set; } = new List<UserResponse>();
    }

    public class SimpleCourseStats
    {
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int PendingCourses { get; set; }
        public int InactiveCourses { get; set; }
        public List<CourseListDto>? RecentCourses { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CourseListDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public string? ThumbnailURL { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }

    public class CourseResponseDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public string? ContentURL { get; set; }
        public string? ThumbnailURL { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }

    public class UpdateUserRequest
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserID { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // Common API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class PaginatedApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<T>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
