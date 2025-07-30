using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using DrugUserPreventionUI.Configuration;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.Courses;

namespace DrugUserPreventionUI.Pages.Courses
{
    public class MyCoursesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MyCoursesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Properties
        public List<RegistrationListDto> Registrations { get; set; } = new List<RegistrationListDto>();
        public UserLearningDashboardDto? Dashboard { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }
        public RegistrationFilterDto FilterModel { get; set; } = new RegistrationFilterDto();

        // Helper properties
        public string UserName => HttpContext.Session.GetString("user_name") ?? "User";
        public string UserRole => HttpContext.Session.GetString("user_role") ?? "Member";
        public bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);

        public async Task<IActionResult> OnGetAsync(
            int pageIndex = 1, int pageSize = 12, string? message = null, string? messageType = null,
            string? searchKeyword = null, string? status = null, string? sortBy = null)
        {
            // Check authentication
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập để xem khóa học của bạn", messageType = "warning" });
            }

            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            // Set filter parameters
            FilterModel.PageIndex = pageIndex;
            FilterModel.PageSize = pageSize;
            FilterModel.SearchKeyword = searchKeyword;
            FilterModel.Status = status;
            FilterModel.SortBy = sortBy ?? "RegistrationDate";

            try
            {
                var client = GetAuthenticatedClient();

                // Load dashboard data
                await LoadDashboard(client);

                // Load user's course registrations
                await LoadMyRegistrations(client);
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUnregisterAsync(int courseId)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn", messageType = "warning" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{ApiUrlHelper.GetCoursesApiUrl()}/{courseId}/unregister");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/MyCourses", new { message = "Hủy đăng ký khóa học thành công!", messageType = "success" });
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

                    var errorMessage = errorResponse?.Message ?? "Không thể hủy đăng ký khóa học";
                    return RedirectToPage("/MyCourses", new { message = errorMessage, messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/MyCourses", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostUpdateProgressAsync(int courseId, decimal progressPercentage, string? status = null)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn", messageType = "warning" });
            }

            try
            {
                var client = GetAuthenticatedClient();

                var updateDto = new UpdateProgressDto
                {
                    CourseID = courseId,
                    ProgressPercentage = progressPercentage,
                    Status = status
                };

                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{ApiUrlHelper.GetLearningApiUrl()}/progress", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/MyCourses", new { message = "Cập nhật tiến độ thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/MyCourses", new { message = $"Lỗi cập nhật tiến độ: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/MyCourses", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
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

        private async Task LoadDashboard(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{ApiUrlHelper.GetLearningApiUrl()}/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserLearningDashboardDto>>();
                    Dashboard = apiResponse?.Data;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expired, will be handled by redirect in OnGetAsync
                    return;
                }
            }
            catch (Exception)
            {
                // Dashboard is optional, don't fail the whole page
                Dashboard = null;
            }
        }

        private async Task LoadMyRegistrations(HttpClient client)
        {
            var queryParams = new List<string>();

            if (FilterModel.PageIndex > 0) queryParams.Add($"pageIndex={FilterModel.PageIndex}");
            if (FilterModel.PageSize > 0) queryParams.Add($"pageSize={FilterModel.PageSize}");
            if (!string.IsNullOrEmpty(FilterModel.SearchKeyword)) queryParams.Add($"searchKeyword={Uri.EscapeDataString(FilterModel.SearchKeyword)}");
            if (!string.IsNullOrEmpty(FilterModel.Status)) queryParams.Add($"status={FilterModel.Status}");
            if (!string.IsNullOrEmpty(FilterModel.SortBy)) queryParams.Add($"sortBy={FilterModel.SortBy}");
            if (FilterModel.SortDescending) queryParams.Add($"sortDescending={FilterModel.SortDescending}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var response = await client.GetAsync($"{ApiUrlHelper.GetLearningApiUrl()}/my-courses{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<RegistrationListDto>>();
                if (apiResponse?.Data != null)
                {
                    Registrations.AddRange(apiResponse.Data);
                    PaginationInfo = apiResponse.Pagination;

                    ViewData["CurrentPage"] = PaginationInfo.CurrentPage;
                    ViewData["TotalPages"] = PaginationInfo.TotalPages;
                    ViewData["TotalItems"] = PaginationInfo.TotalItems;
                    ViewData["HasPreviousPage"] = PaginationInfo.HasPreviousPage;
                    ViewData["HasNextPage"] = PaginationInfo.HasNextPage;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                return;
            }
        }

        // Utility methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Enrolled" => "bg-info",
                "InProgress" => "bg-warning",
                "Completed" => "bg-success",
                "Dropped" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        public string GetStatusIcon(string status)
        {
            return status switch
            {
                "Enrolled" => "fa-user-plus",
                "InProgress" => "fa-play",
                "Completed" => "fa-check",
                "Dropped" => "fa-pause",
                _ => "fa-question"
            };
        }

        public string GetStatusText(string status)
        {
            return status switch
            {
                "Enrolled" => "Đã đăng ký",
                "InProgress" => "Đang học",
                "Completed" => "Hoàn thành",
                "Dropped" => "Đã dừng",
                _ => "Không xác định"
            };
        }

        public string GetProgressBarClass(string status)
        {
            return status switch
            {
                "Enrolled" => "bg-info",
                "InProgress" => "bg-warning",
                "Completed" => "bg-success",
                "Dropped" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        public bool CanContinueLearning(string status)
        {
            return status == "InProgress" || status == "Enrolled";
        }

        public bool IsCompleted(string status)
        {
            return status == "Completed";
        }

        public string GetNextActionText(string status, decimal progress)
        {
            return status switch
            {
                "Completed" => "Xem chứng chỉ",
                "InProgress" => progress > 0 ? "Tiếp tục học" : "Bắt đầu học",
                "Enrolled" => "Bắt đầu học",
                "Dropped" => "Đăng ký lại",
                _ => "Xem chi tiết"
            };
        }

        public string GetNextActionIcon(string status, decimal progress)
        {
            return status switch
            {
                "Completed" => "fa-certificate",
                "InProgress" => progress > 0 ? "fa-play" : "fa-play-circle",
                "Enrolled" => "fa-play-circle",
                "Dropped" => "fa-redo",
                _ => "fa-eye"
            };
        }

        public string GetNextActionUrl(string status, int courseId)
        {
            return status switch
            {
                "Completed" => $"/Courses/Certificate/{courseId}",
                "InProgress" or "Enrolled" => $"/Courses/View/{courseId}",
                "Dropped" => $"/Courses?action=detail&id={courseId}",
                _ => $"/Courses?action=detail&id={courseId}"
            };
        }

        public string GetStatusDescription(string status)
        {
            return status switch
            {
                "Enrolled" => "Bạn đã đăng ký khóa học này nhưng chưa bắt đầu học",
                "InProgress" => "Bạn đang trong quá trình học khóa học này",
                "Completed" => "Chúc mừng! Bạn đã hoàn thành khóa học này",
                "Dropped" => "Bạn đã dừng học khóa học này",
                _ => "Trạng thái không xác định"
            };
        }

        public int GetCompletedLessons(decimal progressPercentage, int totalLessons)
        {
            return (int)Math.Floor(progressPercentage / 100 * totalLessons);
        }

        public bool ShouldShowContinueButton(string status)
        {
            return status == "InProgress" || status == "Enrolled";
        }

        public bool ShouldShowCertificateButton(string status)
        {
            return status == "Completed";
        }

        public bool ShouldShowUnregisterButton(string status)
        {
            return status == "InProgress" || status == "Enrolled";
        }

        public string GetTimeAgo(DateTime date)
        {
            var timeAgo = DateTime.Now - date;

            if (timeAgo.Days > 30)
                return $"{timeAgo.Days / 30} tháng trước";
            else if (timeAgo.Days > 0)
                return $"{timeAgo.Days} ngày trước";
            else if (timeAgo.Hours > 0)
                return $"{timeAgo.Hours} giờ trước";
            else if (timeAgo.Minutes > 0)
                return $"{timeAgo.Minutes} phút trước";
            else
                return "Vừa xong";
        }

        public string GetEstimatedTimeToComplete(decimal currentProgress, int totalLessons, TimeSpan? averageLessonDuration = null)
        {
            if (currentProgress >= 100) return "Đã hoàn thành";

            var remainingLessons = totalLessons - GetCompletedLessons(currentProgress, totalLessons);

            if (averageLessonDuration.HasValue)
            {
                var estimatedTime = TimeSpan.FromTicks(remainingLessons * averageLessonDuration.Value.Ticks);
                if (estimatedTime.TotalHours > 1)
                    return $"Còn khoảng {(int)estimatedTime.TotalHours}h {estimatedTime.Minutes}m";
                else
                    return $"Còn khoảng {estimatedTime.Minutes}m";
            }

            return $"Còn {remainingLessons} bài học";
        }
    }
}