using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using DrugUserPreventionUI.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages.AdminDashboard
{
    public class AdminDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiConfiguration _apiConfig;

        public AdminDashboardModel(IHttpClientFactory httpClientFactory, ApiConfiguration apiConfig)
        {
            _httpClientFactory = httpClientFactory;
            _apiConfig = apiConfig;
        }

        public List<UserResponse> Users { get; set; } = new List<UserResponse>();
        public UserResponse? UserDetail { get; set; }
        public UserStatisticsResponse? Statistics { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }

        [BindProperty]
        public CreateUserForm UserForm { get; set; } = new CreateUserForm();

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

        public async Task<IActionResult> OnGetAsync(
            string? action = null,
            int? id = null,
            int pageIndex = 1,
            int pageSize = 10,
            string? message = null,
            string? messageType = null,
            string? roleFilter = null,
            string? searchTerm = null
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

            // Check if user is Admin
            var userRole = GetUserRole();
            if (userRole != "Admin")
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

            // Populate ProfileForm with current user data when action is profile
            if (CurrentAction == "profile")
            {
                var currentUser = GetCurrentUser();
                if (currentUser != null)
                {
                    ProfileForm.FullName = currentUser.DisplayName ?? "";
                    ProfileForm.Email = currentUser.Email ?? "";
                    // Phone, DateOfBirth, Gender would need to be loaded from API if needed
                }
            }

            // Store filter values for view
            ViewData["RoleFilter"] = roleFilter;
            ViewData["SearchTerm"] = searchTerm;

            try
            {
                var client = GetAuthenticatedClient();

                // Load statistics
                await LoadStatistics(client);

                // Load users list with filters
                await LoadUsersList(client, pageIndex, pageSize, roleFilter, searchTerm);

                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadUserDetail(client, id.Value);
                        }
                        break;
                    case "edit":
                        if (id.HasValue)
                        {
                            await LoadUserForEdit(client, id.Value);
                        }
                        break;
                    case "add":
                        UserForm = new CreateUserForm();
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

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = "Bạn không có quyền tạo tài khoản.", messageType = "error" }
                );
            }

            // Only validate UserForm fields for create user action
            var createUserErrors = new List<string>();
            var userFormPrefix = "UserForm.";

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith(userFormPrefix)))
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Create User Field Error - {key}: {error.ErrorMessage}");
                        createUserErrors.Add(error.ErrorMessage);
                    }
                }
            }

            if (createUserErrors.Any())
            {
                Message = "Dữ liệu không hợp lệ: " + string.Join(", ", createUserErrors);
                MessageType = "error";

                await LoadStatistics(GetAuthenticatedClient());
                await LoadUsersList(GetAuthenticatedClient(), 1, 10);
                CurrentAction = "add";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var createRequest = new CreateUserRequest
                {
                    FullName = UserForm.FullName,
                    Username = UserForm.Username,
                    Email = UserForm.Email,
                    Password = UserForm.Password,
                    Phone = UserForm.Phone,
                    DateOfBirth = UserForm.DateOfBirth,
                    Gender = UserForm.Gender,
                    Role = UserForm.Role,
                    AvatarUrl = UserForm.AvatarUrl,
                };

                var json = JsonSerializer.Serialize(
                    createRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiConfig.AdminApiUrl}/create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return RedirectToPage(
                        "/AdminDashboard",
                        new
                        {
                            message = apiResponse?.Message ?? "Tạo tài khoản thành công!",
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
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Message =
                        $"Lỗi {(int)response.StatusCode}: {errorResponse?.Message ?? response.ReasonPhrase}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi không mong muốn: {ex.Message}";
                MessageType = "error";
            }

            await LoadStatistics(GetAuthenticatedClient());
            await LoadUsersList(GetAuthenticatedClient(), 1, 10);
            CurrentAction = "add";
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateUserAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new
                    {
                        message = "Bạn không có quyền cập nhật tài khoản.",
                        messageType = "error",
                    }
                );
            }

            // Only validate UserForm fields for update user action
            var updateUserErrors = new List<string>();
            var userFormPrefix = "UserForm.";

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith(userFormPrefix)))
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Update User Field Error - {key}: {error.ErrorMessage}");
                        updateUserErrors.Add(error.ErrorMessage);
                    }
                }
            }

            if (updateUserErrors.Any())
            {
                Message = "Dữ liệu không hợp lệ: " + string.Join(", ", updateUserErrors);
                MessageType = "error";

                await LoadStatistics(GetAuthenticatedClient());
                await LoadUsersList(GetAuthenticatedClient(), 1, 10);
                CurrentAction = "edit";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var updateRequest = new UpdateUserRequest
                {
                    UserID = id,
                    FullName = UserForm.FullName,
                    Username = UserForm.Username,
                    Email = UserForm.Email,
                    Phone = UserForm.Phone,
                    DateOfBirth = UserForm.DateOfBirth,
                    Gender = UserForm.Gender,
                    Role = UserForm.Role,
                    AvatarUrl = UserForm.AvatarUrl,
                    Status = "Active", // Default to Active when updating
                    IsEmailVerified =
                        true // Admin can set this
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
                        "/AdminDashboard",
                        new { message = "Cập nhật tài khoản thành công!", messageType = "success" }
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
                    Message = $"Lỗi khi cập nhật: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadStatistics(GetAuthenticatedClient());
            await LoadUsersList(GetAuthenticatedClient(), 1, 10);
            CurrentAction = "edit";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = "Bạn không có quyền xóa tài khoản.", messageType = "error" }
                );
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{_apiConfig.AdminApiUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/AdminDashboard",
                        new { message = "Xóa tài khoản thành công!", messageType = "success" }
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
                        "/AdminDashboard",
                        new { message = $"Lỗi khi xóa: {errorContent}", messageType = "error" }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" }
                );
            }
        }

        public async Task<IActionResult> OnPostBanUserAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
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
                        "/AdminDashboard",
                        new { message = "Đã cấm tài khoản thành công!", messageType = "success" }
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
                        "/AdminDashboard",
                        new
                        {
                            message = $"Lỗi khi cấm tài khoản: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" }
                );
            }
        }

        public async Task<IActionResult> OnPostUnbanUserAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
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
                        "/AdminDashboard",
                        new { message = "Đã bỏ cấm tài khoản thành công!", messageType = "success" }
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
                        "/AdminDashboard",
                        new
                        {
                            message = $"Lỗi khi bỏ cấm tài khoản: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" }
                );
            }
        }

        public async Task<IActionResult> OnPostVerifyEmailAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage(
                    "/Login",
                    new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" }
                );
            }

            if (GetUserRole() != "Admin")
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = "Bạn không có quyền xác thực email.", messageType = "error" }
                );
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{_apiConfig.AdminApiUrl}/verify-email/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        "/AdminDashboard",
                        new { message = "Đã xác thực email thành công!", messageType = "success" }
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
                        "/AdminDashboard",
                        new
                        {
                            message = $"Lỗi khi xác thực email: {errorContent}",
                            messageType = "error",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage(
                    "/AdminDashboard",
                    new { message = $"Lỗi: {ex.Message}", messageType = "error" }
                );
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            // Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/AdminDashboard", new { 
                    message = "Vui lòng đăng nhập để truy cập trang này.", 
                    messageType = "warning" 
                });
            }

            // Check if user is Admin
            var userRole = GetUserRole();
            if (userRole != "Admin")
            {
                return RedirectToPage("/AdminDashboard", new { 
                    message = "Bạn không có quyền thực hiện chức năng này.", 
                    messageType = "error" 
                });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToPage("/AdminDashboard", new { 
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
                    return RedirectToPage("/AdminDashboard", new { 
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

                var response = await client.PutAsync($"{_apiConfig.UserApiUrl}/{currentUser.UserId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/AdminDashboard", new { 
                        action = "profile", 
                        message = "Cập nhật thông tin thành công!", 
                        messageType = "success" 
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/AdminDashboard", new { 
                        action = "profile", 
                        message = $"Lỗi cập nhật: {response.StatusCode}", 
                        messageType = "error" 
                    });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/AdminDashboard", new { 
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

        private async Task LoadStatistics(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/statistics");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect(
                        "/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning"
                    );
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserStatisticsResponse>
                    >();
                    Statistics = apiResponse?.Data;
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Lỗi khi tải thống kê: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadUsersList(
            HttpClient client,
            int pageIndex,
            int pageSize,
            string? roleFilter = null,
            string? searchTerm = null
        )
        {
            try
            {
                string url;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url =
                        $"{_apiConfig.AdminApiUrl}/search?name={Uri.EscapeDataString(searchTerm)}&page={pageIndex}&pageSize={pageSize}";
                }
                else if (!string.IsNullOrEmpty(roleFilter))
                {
                    url = $"{_apiConfig.AdminApiUrl}/role/{roleFilter}";
                }
                else
                {
                    url = $"{_apiConfig.AdminApiUrl}/search?page={pageIndex}&pageSize={pageSize}";
                }

                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect(
                        "/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning"
                    );
                    return;
                }

                response.EnsureSuccessStatusCode();

                if (!string.IsNullOrEmpty(roleFilter) && string.IsNullOrEmpty(searchTerm))
                {
                    // For role filter, we get a simple list
                    var roleApiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<List<UserResponse>>
                    >();
                    if (roleApiResponse?.Data != null)
                    {
                        Users.AddRange(
                            roleApiResponse.Data.Where(u =>
                                u.Role == "Staff" || u.Role == "Consultant"
                            )
                        );

                        // Create simple pagination info for role filter
                        var totalItems = Users.Count;
                        ViewData["CurrentPage"] = 1;
                        ViewData["TotalPages"] = 1;
                        ViewData["TotalItems"] = totalItems;
                        ViewData["HasPreviousPage"] = false;
                        ViewData["HasNextPage"] = false;
                    }
                }
                else
                {
                    // For search or default listing, we get paginated response
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<PaginatedUsersResponse>
                    >();
                    if (apiResponse?.Data != null)
                    {
                        Users.AddRange(
                            apiResponse.Data.Users.Where(u =>
                                u.Role == "Staff" || u.Role == "Consultant" || u.Role == "Member"
                            )
                        );

                        ViewData["CurrentPage"] = apiResponse.Data.CurrentPage;
                        ViewData["TotalPages"] = apiResponse.Data.TotalPages;
                        ViewData["TotalItems"] = apiResponse.Data.TotalCount;
                        ViewData["HasPreviousPage"] = apiResponse.Data.CurrentPage > 1;
                        ViewData["HasNextPage"] =
                            apiResponse.Data.CurrentPage < apiResponse.Data.TotalPages;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Lỗi khi tải danh sách người dùng: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadUserDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserResponse>
                    >();
                    UserDetail = apiResponse?.Data;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect(
                        "/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning"
                    );
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải chi tiết người dùng: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadUserForEdit(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.AdminApiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<
                        ApiResponse<UserResponse>
                    >();
                    if (apiResponse?.Data != null)
                    {
                        var user = apiResponse.Data;
                        UserForm = new CreateUserForm
                        {
                            UserID = user.UserID,
                            FullName = user.FullName,
                            Username = user.Username,
                            Email = user.Email,
                            Phone = user.Phone,
                            DateOfBirth = user.DateOfBirth,
                            Gender = user.Gender,
                            Role = user.Role,
                            AvatarUrl = user.AvatarUrl,
                        };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect(
                        "/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning"
                    );
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin người dùng để chỉnh sửa: {ex.Message}";
                MessageType = "error";
            }
        }
    }

    // DTOs for Admin Dashboard
    public class CreateUserForm
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, ErrorMessage = "Username không được quá 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string Role { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL Avatar không hợp lệ")]
        public string? AvatarUrl { get; set; }
    }

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

    public class PaginatedUsersResponse
    {
        public List<UserResponse> Users { get; set; } = new List<UserResponse>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
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
}
