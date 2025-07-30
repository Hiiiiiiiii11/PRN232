using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using DrugUserPreventionUI.Models.CourseDashboard;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages.CourseDashboard
{
    public class CourseDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CourseDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CourseListDto> Courses { get; set; } = new List<CourseListDto>();
        public CourseResponseDto? CourseDetail { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }

        [BindProperty]
        public CreateCourseDto CourseForm { get; set; } = new CreateCourseDto();

        [BindProperty]
        public ProfileForm ProfileForm { get; set; } = new ProfileForm();

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

        // ✅ ADDED: Role-based permission checks
        public bool UserCanCreateCourse()
        {
            var role = GetUserRole();
            return role == "Consultant" || role == "Admin";
        }

        public bool UserCanEditCourse(string? creatorName = null)
        {
            var role = GetUserRole();
            var displayName = GetDisplayName();

            // Admin can edit all courses
            if (role == "Admin") return true;

            // Consultant can edit their own courses
            if (role == "Consultant" && creatorName == displayName) return true;

            return false;
        }

        public bool UserCanDeleteCourse(string? creatorName = null)
        {
            var role = GetUserRole();
            var displayName = GetDisplayName();

            // Admin can delete all courses
            if (role == "Admin") return true;

            // Consultant can delete their own courses
            if (role == "Consultant" && creatorName == displayName) return true;

            return false;
        }

        public bool UserCanUpdateStatus()
        {
            var role = GetUserRole();
            return role == "Manager" || role == "Staff" || role == "Admin";
        }

        public bool UserCanApproveCourse()
        {
            var role = GetUserRole();
            return role == "Manager" || role == "Admin";
        }

        public async Task<IActionResult> OnGetAsync(string? action = null, int? id = null,
            int pageIndex = 1, int pageSize = 10, string? message = null, string? messageType = null)
        {
            // Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập để truy cập trang này.", messageType = "warning" });
            }

            // Check if user has permission to access dashboard
            var userRole = GetUserRole();
            if (userRole == "Member" || userRole == "Guest")
            {
                return RedirectToPage("/Courses", new { message = "Bạn không có quyền truy cập trang quản lý.", messageType = "error" });
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
                await LoadCoursesList(client, pageIndex, pageSize);

                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadCourseDetail(client, id.Value);
                        }
                        break;
                    case "edit":
                        if (id.HasValue)
                        {
                            await LoadCourseForEdit(client, id.Value);
                        }
                        break;
                    case "add":
                        if (!UserCanCreateCourse())
                        {
                            return RedirectToPage("/CourseDashboard/CourseDashboard",
                                new { message = "Bạn không có quyền tạo khóa học mới.", messageType = "error" });
                        }
                        CourseForm = new CreateCourseDto();
                        break;
                    case "profile":
                        // Populate ProfileForm with current user data
                        var currentUser = GetCurrentUser();
                        if (currentUser != null)
                        {
                            ProfileForm.FullName = currentUser.DisplayName ?? "";
                            ProfileForm.Email = currentUser.Email ?? "";
                            // Phone, DateOfBirth, Gender would need to be loaded from API if needed
                        }
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

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanCreateCourse())
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Bạn không có quyền tạo khóa học mới.", messageType = "error" });
            }

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                Message = "Dữ liệu không hợp lệ: " + string.Join(", ", validationErrors);
                MessageType = "error";

                await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
                CurrentAction = "add";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(CourseForm, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(ApiUrlHelper.GetCoursesApiUrl(), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CourseResponseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return RedirectToPage("/CourseDashboard/CourseDashboard",
                            new { message = "Tạo khóa học thành công! (Đang chờ phê duyệt)", messageType = "success" });
                    }
                    else
                    {
                        Message = $"Lỗi API: {apiResponse?.Message ?? "Không thể tạo khóa học"}";
                        MessageType = "error";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Message = $"Lỗi {(int)response.StatusCode}: {errorResponse?.Message ?? response.ReasonPhrase}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi không mong muốn: {ex.Message}";
                MessageType = "error";
            }

            await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
            CurrentAction = "add";
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            // Load course first to check permissions
            var client = GetAuthenticatedClient();
            var courseResponse = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");
            if (!courseResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Không tìm thấy khóa học.", messageType = "error" });
            }

            var courseApiResponse = await courseResponse.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
            var course = courseApiResponse?.Data;

            if (!UserCanEditCourse(course?.CreatorName))
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Bạn không có quyền chỉnh sửa khóa học này.", messageType = "error" });
            }

            if (!ModelState.IsValid)
            {
                await LoadCoursesList(client, 1, 10);
                CurrentAction = "edit";
                return Page();
            }

            try
            {
                var updateDto = new UpdateCourseDto
                {
                    CourseID = id,
                    Title = CourseForm.Title,
                    Description = CourseForm.Description,
                    TargetGroup = CourseForm.TargetGroup,
                    AgeGroup = CourseForm.AgeGroup,
                    ContentURL = CourseForm.ContentURL,
                    ThumbnailURL = CourseForm.ThumbnailURL
                };

                var json = JsonSerializer.Serialize(updateDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = "Cập nhật khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
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

            await LoadCoursesList(client, 1, 10);
            CurrentAction = "edit";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            // Load course first to check permissions
            var client = GetAuthenticatedClient();
            var courseResponse = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");
            if (!courseResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Không tìm thấy khóa học.", messageType = "error" });
            }

            var courseApiResponse = await courseResponse.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
            var course = courseApiResponse?.Data;

            if (!UserCanDeleteCourse(course?.CreatorName))
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Bạn không có quyền xóa khóa học này.", messageType = "error" });
            }

            try
            {
                var response = await client.DeleteAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = "Xóa khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = $"Lỗi khi xóa: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, bool isActive)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanUpdateStatus())
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Bạn không có quyền cập nhật trạng thái khóa học.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isActive);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}/toggle-status", content);

                if (response.IsSuccessStatusCode)
                {
                    var statusText = isActive ? "kích hoạt" : "vô hiệu hóa";
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = $"Đã {statusText} khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = $"Lỗi khi cập nhật trạng thái: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, bool isAccept)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanApproveCourse())
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = "Bạn không có quyền duyệt khóa học.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isAccept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}/approve", content);

                if (response.IsSuccessStatusCode)
                {
                    var approvalText = isAccept ? "duyệt" : "từ chối";
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = $"Đã {approvalText} khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard",
                        new { message = $"Lỗi khi duyệt khóa học: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            // Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                    message = "Vui lòng đăng nhập để truy cập trang này.", 
                    messageType = "warning" 
                });
            }

            // Check if user is Consultant or has appropriate role
            var userRole = GetUserRole();
            if (userRole != "Consultant" && userRole != "Staff" && userRole != "Manager" && userRole != "Admin")
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                    message = "Bạn không có quyền thực hiện chức năng này.", 
                    messageType = "error" 
                });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                    action = "profile", 
                    message = "Vui lòng kiểm tra thông tin nhập vào.", 
                    messageType = "error" 
                });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var currentUser = GetCurrentUser();
                
                if (currentUser == null)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                        message = "Không thể xác định thông tin người dùng.", 
                        messageType = "error" 
                    });
                }

                var updateRequest = new
                {
                    UserId = currentUser.UserId,
                    FullName = ProfileForm.FullName,
                    Email = ProfileForm.Email,
                    Phone = ProfileForm.Phone,
                    DateOfBirth = ProfileForm.DateOfBirth,
                    Gender = ProfileForm.Gender
                };

                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{ApiUrlHelper.GetUserApiUrl()}/{currentUser.UserId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                        action = "profile", 
                        message = "Cập nhật thông tin thành công!", 
                        messageType = "success" 
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                        action = "profile", 
                        message = $"Lỗi cập nhật: {response.StatusCode}", 
                        messageType = "error" 
                    });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { 
                    action = "profile", 
                    message = $"Lỗi: {ex.Message}", 
                    messageType = "error" 
                });
            }
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

        private async Task LoadCoursesList(HttpClient client, int pageIndex, int pageSize)
        {
            var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";

            try
            {
                var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}{queryString}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                    return;
                }

                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<CourseListDto>>();
                if (apiResponse?.Data != null)
                {
                    Courses.AddRange(apiResponse.Data);
                    PaginationInfo = apiResponse.Pagination;

                    ViewData["CurrentPage"] = PaginationInfo.CurrentPage;
                    ViewData["TotalPages"] = PaginationInfo.TotalPages;
                    ViewData["TotalItems"] = PaginationInfo.TotalItems;
                    ViewData["HasPreviousPage"] = PaginationInfo.HasPreviousPage;
                    ViewData["HasNextPage"] = PaginationInfo.HasNextPage;
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Lỗi khi tải danh sách khóa học: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCourseDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
                    CourseDetail = apiResponse?.Data;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải chi tiết khóa học: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCourseForEdit(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
                    if (apiResponse?.Data != null)
                    {
                        var course = apiResponse.Data;

                        // Check permission before loading for edit
                        if (!UserCanEditCourse(course.CreatorName))
                        {
                            Message = "Bạn không có quyền chỉnh sửa khóa học này.";
                            MessageType = "error";
                            return;
                        }

                        CourseForm = new CreateCourseDto
                        {
                            Title = course.Title,
                            Description = course.Description ?? string.Empty,
                            TargetGroup = course.TargetGroup ?? string.Empty,
                            AgeGroup = course.AgeGroup ?? string.Empty,
                            ContentURL = course.ContentURL,
                            ThumbnailURL = course.ThumbnailURL
                        };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin khóa học để chỉnh sửa: {ex.Message}";
                MessageType = "error";
            }
        }
    }
}