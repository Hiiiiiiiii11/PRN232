using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.CourseDashboard;
using DrugUserPreventionUI.Models.Courses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;
using System.Text;
using System.Text.Json;

namespace DrugUserPreventionUI.Pages.Courses
{
    public class CoursesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CoursesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Properties
        public List<CourseListDto> Courses { get; set; } = new List<CourseListDto>();
        public Models.CourseDashboard.CourseResponseDto? CourseDetail { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }
        public CourseFilterDto FilterModel { get; set; } = new CourseFilterDto();

        // Helper properties
        public string UserName => HttpContext.Session.GetString("user_name") ?? "User";
        public string UserRole => HttpContext.Session.GetString("user_role") ?? "Member";
        public bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);

        public async Task<IActionResult> OnGetAsync(string? action = null, int? id = null,
            int pageIndex = 1, int pageSize = 12, string? message = null, string? messageType = null,
            string? searchKeyword = null, string? targetGroup = null, string? ageGroup = null,
            DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null)
        {
            CurrentAction = action?.ToLower();

            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            // Set filter parameters
            FilterModel.PageIndex = pageIndex;
            FilterModel.PageSize = pageSize;
            FilterModel.SearchKeyword = searchKeyword;
            FilterModel.TargetGroup = targetGroup;
            FilterModel.AgeGroup = ageGroup;
            FilterModel.StartDate = startDate;
            FilterModel.EndDate = endDate;
            FilterModel.IsActive = isActive;

            try
            {
                var client = GetHttpClient();

                // Load courses with filters
                await LoadCoursesList(client);

                // Handle specific actions
                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadCourseDetail(client, id.Value);
                        }
                        break;
                    case "register":
                        if (id.HasValue && IsAuthenticated)
                        {
                            return await RegisterForCourse(id.Value);
                        }
                        break;
                    case "unregister":
                        if (id.HasValue && IsAuthenticated)
                        {
                            return await UnregisterFromCourse(id.Value);
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

        public async Task<IActionResult> OnPostRegisterAsync(int courseId)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập để đăng ký khóa học", messageType = "warning" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/register", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Courses/Courses", new { hanler = "Register", message = "Đăng ký khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var errorMessage = errorResponse?.Message ?? "Không thể đăng ký khóa học";
                    return RedirectToPage("/Courses/Courses", new { message = errorMessage, messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Courses/Courses", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostUnregisterAsync(int courseId)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập", messageType = "warning" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/unregister");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Courses/Courses", new { hanler = "Unregister", message = "Hủy đăng ký khóa học thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Courses/Courses", new { message = $"Lỗi khi hủy đăng ký: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Courses/Courses", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // Helper methods
        private HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient();
        }

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

        private async Task LoadCoursesList(HttpClient client)
        {
            var queryParams = new List<string>();

            if (FilterModel.PageIndex > 0) queryParams.Add($"pageIndex={FilterModel.PageIndex}");
            if (FilterModel.PageSize > 0) queryParams.Add($"pageSize={FilterModel.PageSize}");
            if (!string.IsNullOrEmpty(FilterModel.SearchKeyword)) queryParams.Add($"searchKeyword={Uri.EscapeDataString(FilterModel.SearchKeyword)}");
            if (!string.IsNullOrEmpty(FilterModel.TargetGroup)) queryParams.Add($"targetGroup={Uri.EscapeDataString(FilterModel.TargetGroup)}");
            if (!string.IsNullOrEmpty(FilterModel.AgeGroup)) queryParams.Add($"ageGroup={Uri.EscapeDataString(FilterModel.AgeGroup)}");
            if (FilterModel.FromDate.HasValue) queryParams.Add($"fromDate={FilterModel.FromDate.Value:yyyy-MM-dd}");
            if (FilterModel.ToDate.HasValue) queryParams.Add($"toDate={FilterModel.ToDate.Value:yyyy-MM-dd}");
            if (FilterModel.IsActive.HasValue) queryParams.Add($"isActive={FilterModel.IsActive.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<CourseListDto>>();
                if (apiResponse?.Data != null)
                {
                    Courses.AddRange(apiResponse.Data);
                    PaginationInfo = apiResponse.Pagination;

                    // Set ViewData for pagination
                    ViewData["CurrentPage"] = PaginationInfo.CurrentPage;
                    ViewData["TotalPages"] = PaginationInfo.TotalPages;
                    ViewData["TotalItems"] = PaginationInfo.TotalItems;
                    ViewData["HasPreviousPage"] = PaginationInfo.HasPreviousPage;
                    ViewData["HasNextPage"] = PaginationInfo.HasNextPage;
                }
            }
        }

        private async Task LoadCourseDetail(HttpClient client, int id)
        {
            var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Models.CourseDashboard.CourseResponseDto>>();
                CourseDetail = apiResponse?.Data;
            }
        }

        private async Task<IActionResult> RegisterForCourse(int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/register", null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Courses", new { message = "Đăng ký khóa học thành công!", messageType = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return RedirectToPage("/Courses", new { message = $"Lỗi đăng ký: {errorContent}", messageType = "error" });
            }
        }

        private async Task<IActionResult> UnregisterFromCourse(int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/unregister");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Courses", new { message = "Hủy đăng ký thành công!", messageType = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return RedirectToPage("/Courses", new { message = $"Lỗi hủy đăng ký: {errorContent}", messageType = "error" });
            }
        }

        public async Task<bool> IsUserRegisteredForCourse(int courseId)
        {
            if (!IsAuthenticated) return false;

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/is-registered");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return apiResponse?.Data ?? false;
                }
            }
            catch (Exception)
            {
                // Ignore errors for this check
            }

            return false;
        }
    }
}