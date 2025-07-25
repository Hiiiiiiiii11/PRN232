using System.Text;
using System.Text.Json;
using DrugUserPreventionUI.Models.Categories;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.NewsArticles;
using DrugUserPreventionUI.Models.Tags;
using DrugUserPreventionUI.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DrugUsePrevention.Pages.Staff
{
    public class StaffDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BASE_API_URL = "https://localhost:7045/api";

        public StaffDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Properties
        public string? CurrentSection { get; set; } = "dashboard";
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        // User Info Properties
        public string CurrentUserRole => GetUserRole();
        public string CurrentUserDisplayName => GetDisplayName();
        public int CurrentUserId => GetCurrentUserId();

        // Data Properties
        public DashboardStatsDto DashboardStats { get; set; } = new DashboardStatsDto();
        public List<UserResponse> Members { get; set; } = new List<UserResponse>();
        public List<NewsArticleDto> NewsArticles { get; set; } = new List<NewsArticleDto>();
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();

        // Form Properties
        [BindProperty]
        public CreateMemberDto MemberForm { get; set; } = new CreateMemberDto();

        [BindProperty]
        public CreateCategoryDto CategoryForm { get; set; } = new CreateCategoryDto();

        [BindProperty]
        public CreateTagDto TagForm { get; set; } = new CreateTagDto();

        [BindProperty]
        public UpdateProfileDto ProfileForm { get; set; } = new UpdateProfileDto();

        [BindProperty]
        public ChangePasswordDto ChangePasswordForm { get; set; } = new ChangePasswordDto();

        // Helper method for SAFE redirects - FIXED
        private IActionResult SafeRedirectToCurrentPage(
            string section,
            string message,
            string messageType
        )
        {
            // Use current page path without absolute routing
            return RedirectToPage(
                new
                {
                    section,
                    message,
                    messageType,
                }
            );
        }

        // Authentication helpers
        private LoginModel GetLoginModel()
        {
            var loginModel = new LoginModel(_httpClientFactory);
            loginModel.PageContext = PageContext;
            return loginModel;
        }

        private UserInfoDto? GetCurrentUser() => GetLoginModel().GetCurrentUser();

        private bool IsAuthenticated() => GetLoginModel().IsAuthenticated();

        private string GetUserRole() => GetLoginModel().GetUserRole();

        public string GetDisplayName() => GetLoginModel().GetDisplayName();

        private int GetCurrentUserId() => GetCurrentUser()?.UserID ?? 0;

        // Permission checks
        public bool UserCanAccessStaffDashboard()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        public bool UserCanManageMembers() => UserCanAccessStaffDashboard();

        public bool UserCanManageNews() => UserCanAccessStaffDashboard();

        public bool UserCanManageCategories() => UserCanAccessStaffDashboard();

        public bool UserCanManageTags() => UserCanAccessStaffDashboard();

        // GET Handler
        public async Task<IActionResult> OnGetAsync(
            string? section = null,
            int pageIndex = 1,
            int pageSize = 10,
            string? message = null,
            string? messageType = null
        )
        {
            try
            {
                // Check authentication - Use simpler redirect
                if (!IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                // Check permissions
                if (!UserCanAccessStaffDashboard())
                {
                    return RedirectToPage("/Index");
                }

                CurrentSection = section?.ToLower() ?? "dashboard";

                if (!string.IsNullOrEmpty(message))
                {
                    Message = message;
                    MessageType = messageType ?? "info";
                }

                var client = GetAuthenticatedClient();
                await LoadDashboardStats(client);

                // Load section-specific data
                switch (CurrentSection)
                {
                    case "members":
                        if (UserCanManageMembers())
                            await LoadMembers(client, pageIndex, pageSize);
                        break;
                    case "news":
                        if (UserCanManageNews())
                            await LoadNews(client, pageIndex, pageSize);
                        break;
                    case "categories":
                        if (UserCanManageCategories())
                            await LoadCategories(client, pageIndex, pageSize);
                        break;
                    case "tags":
                        if (UserCanManageTags())
                            await LoadTags(client, pageIndex, pageSize);
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

        // CREATE MEMBER
        public async Task<IActionResult> OnPostCreateMemberAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageMembers())
            {
                Message = "Bạn không có quyền tạo Member mới.";
                MessageType = "error";
                CurrentSection = "members";
                await LoadMembers(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                CurrentSection = "members";
                await LoadMembers(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var createRequest = new CreateUserRequest
                {
                    FullName = MemberForm.FullName,
                    Username = MemberForm.Username,
                    Email = MemberForm.Email,
                    Password = MemberForm.Password,
                    Phone = MemberForm.Phone,
                    Role = "Member",
                };

                var json = JsonSerializer.Serialize(
                    createRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/create", content);

                if (response.IsSuccessStatusCode)
                {
                    return SafeRedirectToCurrentPage(
                        "members",
                        "Tạo Member thành công!",
                        "success"
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Member: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            MemberForm = new CreateMemberDto();
            CurrentSection = "members";
            await LoadMembers(GetAuthenticatedClient(), 1, 10);
            return Page();
        }

        // BAN MEMBER
        public async Task<IActionResult> OnPostBanMemberAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageMembers())
                return SafeRedirectToCurrentPage(
                    "members",
                    "Bạn không có quyền ban Member.",
                    "error"
                );

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/ban/{id}", null);

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "members",
                        "Ban Member thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "members",
                        $"Lỗi khi ban Member: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("members", $"Lỗi: {ex.Message}", "error");
            }
        }

        // UNBAN MEMBER
        public async Task<IActionResult> OnPostUnbanMemberAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageMembers())
                return SafeRedirectToCurrentPage(
                    "members",
                    "Bạn không có quyền unban Member.",
                    "error"
                );

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/unban/{id}", null);

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "members",
                        "Unban Member thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "members",
                        $"Lỗi khi unban Member: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("members", $"Lỗi: {ex.Message}", "error");
            }
        }

        // NEWS HANDLERS
        public async Task<IActionResult> OnPostToggleNewsStatusAsync(int id, string status)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageNews())
                return SafeRedirectToCurrentPage(
                    "news",
                    "Bạn không có quyền thay đổi trạng thái News.",
                    "error"
                );

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(status);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(
                    $"{BASE_API_URL}/NewsArticles/{id}/toggle-status",
                    content
                );

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "news",
                        "Cập nhật trạng thái News thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "news",
                        $"Lỗi khi cập nhật trạng thái: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("news", $"Lỗi: {ex.Message}", "error");
            }
        }

        public async Task<IActionResult> OnPostDeleteNewsAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageNews())
                return SafeRedirectToCurrentPage("news", "Bạn không có quyền xóa News.", "error");

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/NewsArticles/{id}");

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage("news", "Xóa News thành công!", "success");
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "news",
                        $"Lỗi khi xóa News: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("news", $"Lỗi: {ex.Message}", "error");
            }
        }

        // CATEGORY HANDLERS
        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageCategories())
            {
                Message = "Bạn không có quyền tạo Category.";
                MessageType = "error";
                CurrentSection = "categories";
                await LoadCategories(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                CurrentSection = "categories";
                await LoadCategories(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(
                    CategoryForm,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Categories", content);

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "categories",
                        "Tạo Category thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Category: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            CategoryForm = new CreateCategoryDto();
            CurrentSection = "categories";
            await LoadCategories(GetAuthenticatedClient(), 1, 10);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageCategories())
                return SafeRedirectToCurrentPage(
                    "categories",
                    "Bạn không có quyền xóa Category.",
                    "error"
                );

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/Categories/{id}");

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "categories",
                        "Xóa Category thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "categories",
                        $"Lỗi khi xóa Category: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("categories", $"Lỗi: {ex.Message}", "error");
            }
        }

        // TAG HANDLERS
        public async Task<IActionResult> OnPostCreateTagAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageTags())
            {
                Message = "Bạn không có quyền tạo Tag.";
                MessageType = "error";
                CurrentSection = "tags";
                await LoadTags(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                CurrentSection = "tags";
                await LoadTags(GetAuthenticatedClient(), 1, 10);
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(
                    TagForm,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Tags", content);

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage("tags", "Tạo Tag thành công!", "success");
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Tag: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            TagForm = new CreateTagDto();
            CurrentSection = "tags";
            await LoadTags(GetAuthenticatedClient(), 1, 10);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTagAsync(int id)
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!UserCanManageTags())
                return SafeRedirectToCurrentPage("tags", "Bạn không có quyền xóa Tag.", "error");

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/Tags/{id}");

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage("tags", "Xóa Tag thành công!", "success");
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return SafeRedirectToCurrentPage(
                        "tags",
                        $"Lỗi khi xóa Tag: {errorContent}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                return SafeRedirectToCurrentPage("tags", $"Lỗi: {ex.Message}", "error");
            }
        }

        // PROFILE HANDLERS
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                CurrentSection = "profile";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var updateRequest = new UpdateUserRequest
                {
                    UserID = CurrentUserId,
                    FullName = ProfileForm.FullName,
                    Username = ProfileForm.Username,
                    Email = ProfileForm.Email,
                    Phone = ProfileForm.Phone,
                };

                var json = JsonSerializer.Serialize(
                    updateRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{BASE_API_URL}/Admin/update", content);

                if (response.IsSuccessStatusCode)
                    return SafeRedirectToCurrentPage(
                        "profile",
                        "Cập nhật Profile thành công!",
                        "success"
                    );
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi cập nhật Profile: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            CurrentSection = "profile";
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                CurrentSection = "profile";
                return Page();
            }

            if (ChangePasswordForm.NewPassword != ChangePasswordForm.ConfirmPassword)
            {
                Message = "Mật khẩu xác nhận không khớp.";
                MessageType = "error";
                CurrentSection = "profile";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var changePasswordRequest = new ChangePasswordRequest
                {
                    UserID = CurrentUserId,
                    OldPassword = ChangePasswordForm.OldPassword,
                    NewPassword = ChangePasswordForm.NewPassword,
                };

                var json = JsonSerializer.Serialize(
                    changePasswordRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(
                    $"{BASE_API_URL}/Admin/change-password",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    ChangePasswordForm = new ChangePasswordDto();
                    return SafeRedirectToCurrentPage(
                        "profile",
                        "Đổi mật khẩu thành công!",
                        "success"
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

            CurrentSection = "profile";
            return Page();
        }

        // HELPER METHODS
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

        private async Task LoadDashboardStats(HttpClient client)
        {
            try
            {
                // Load user statistics
                var userStatsResponse = await client.GetAsync($"{BASE_API_URL}/Admin/statistics");
                if (userStatsResponse.IsSuccessStatusCode)
                {
                    var userStatsJson = await userStatsResponse.Content.ReadAsStringAsync();
                    var userStatsResult = JsonSerializer.Deserialize<AdminStatsResponse>(
                        userStatsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (userStatsResult?.Data != null)
                    {
                        DashboardStats.TotalMembers = userStatsResult.Data.TotalUsers;
                        DashboardStats.UsersByRole = userStatsResult.Data.UsersByRole;
                        DashboardStats.UsersByStatus = userStatsResult.Data.UsersByStatus;
                    }
                }

                // Load news statistics
                var newsStatsResponse = await client.GetAsync($"{BASE_API_URL}/NewsArticles/stats");
                if (newsStatsResponse.IsSuccessStatusCode)
                {
                    var newsStatsJson = await newsStatsResponse.Content.ReadAsStringAsync();
                    var newsStatsResult = JsonSerializer.Deserialize<NewsStatsResponse>(
                        newsStatsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (newsStatsResult?.Data != null)
                        DashboardStats.TotalNews = newsStatsResult.Data.TotalNewsArticles;
                }

                // Load category statistics
                var categoryStatsResponse = await client.GetAsync(
                    $"{BASE_API_URL}/Categories/stats"
                );
                if (categoryStatsResponse.IsSuccessStatusCode)
                {
                    var categoryStatsJson = await categoryStatsResponse.Content.ReadAsStringAsync();
                    var categoryStatsResult = JsonSerializer.Deserialize<CategoryStatsResponse>(
                        categoryStatsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (categoryStatsResult?.Data != null)
                        DashboardStats.TotalCategories = categoryStatsResult.Data.TotalCategories;
                }

                // Load tags count
                var tagsResponse = await client.GetAsync($"{BASE_API_URL}/Tags");
                if (tagsResponse.IsSuccessStatusCode)
                {
                    var tagsJson = await tagsResponse.Content.ReadAsStringAsync();
                    var tagsResult = JsonSerializer.Deserialize<PaginatedApiResponse<TagDTO>>(
                        tagsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (tagsResult?.Pagination != null)
                        DashboardStats.TotalTags = tagsResult.Pagination.TotalItems;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard stats: {ex.Message}");
            }
        }

        private async Task LoadMembers(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_API_URL}/Admin/role/Member");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AdminDto>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Members = result?.Data?.ToList() ?? new List<UserResponse>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading members: {ex.Message}");
                Members = new List<UserResponse>();
            }
        }

        private async Task LoadNews(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync(
                    $"{BASE_API_URL}/NewsArticles/admin{queryString}"
                );
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<NewsArticleDto>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    NewsArticles = result?.Data?.ToList() ?? new List<NewsArticleDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading news: {ex.Message}");
                NewsArticles = new List<NewsArticleDto>();
            }
        }

        private async Task LoadCategories(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync(
                    $"{BASE_API_URL}/Categories/admin{queryString}"
                );
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<CategoryDTO>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Categories = result?.Data?.ToList() ?? new List<CategoryDTO>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                Categories = new List<CategoryDTO>();
            }
        }

        private async Task LoadTags(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync($"{BASE_API_URL}/Tags{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<TagDTO>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Tags = result?.Data?.ToList() ?? new List<TagDTO>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tags: {ex.Message}");
                Tags = new List<TagDTO>();
            }
        }

        private async Task LoadCurrentUserProfile(HttpClient client)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser != null)
                {
                    ProfileForm = new UpdateProfileDto
                    {
                        FullName = currentUser.FullName,
                        Username = currentUser.Username,
                        Email = currentUser.Email,
                        Phone = currentUser.Phone ?? "",
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
                ProfileForm = new UpdateProfileDto();
            }
        }
    }

    // DTO CLASSES
    public class DashboardStatsDto
    {
        public int TotalMembers { get; set; }
        public int TotalNews { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
        public Dictionary<string, int>? UsersByRole { get; set; } = new();
        public Dictionary<string, int>? UsersByStatus { get; set; } = new();
    }

    public class CreateMemberDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateTagDto
    {
        public string TagName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // API REQUEST CLASSES
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserID { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // RESPONSE CLASSES
    public class AdminStatsResponse
    {
        public UserStatisticsResponse? Data { get; set; }
    }

    public class NewsStatsResponse
    {
        public NewsStatsDto? Data { get; set; }
    }

    public class CategoryStatsResponse
    {
        public CategoryStatsDto? Data { get; set; }
    }

    public class UserStatisticsResponse
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int>? UsersByRole { get; set; }
        public Dictionary<string, int>? UsersByStatus { get; set; }
    }

    public class NewsStatsDto
    {
        public int TotalNewsArticles { get; set; }
    }

    public class CategoryStatsDto
    {
        public int TotalCategories { get; set; }
    }

    public class AdminDto
    {
        public List<UserResponse>? Data { get; set; }
    }

    public class NewsArticleDto
    {
        public int NewsArticleID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string IsActive { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryDTO
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IsActive { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TagDTO
    {
        public int TagID { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserResponse
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class PaginatedApiResponse<T>
    {
        public List<T>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class UserInfoDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    // PLACEHOLDER LOGIN MODEL - REPLACE WITH YOUR ACTUAL ONE
    public class LoginModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PageContext? PageContext { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public UserInfoDto? GetCurrentUser()
        {
            return new UserInfoDto
            {
                UserID = 1,
                FullName = "Staff User",
                Username = "staff",
                Email = "staff@example.com",
                Phone = "",
            };
        }

        public bool IsAuthenticated() => true;

        public string GetUserRole() => "Staff";

        public string GetDisplayName() => GetCurrentUser()?.FullName ?? "Staff User";
    }
}
