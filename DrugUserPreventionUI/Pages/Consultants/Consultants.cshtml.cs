using System.Text.Json;
using System.Text.Json.Serialization;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.Consultants;
using DrugUserPreventionUI.Models.Courses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DrugUserPreventionUI.Pages.Consultants
{
    public class ConsultantsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConsultantsModel> _logger;
        private const string BASE_API_URL = "https://localhost:7045/api/ConsultantUser";

        public ConsultantsModel(
            IHttpClientFactory httpClientFactory,
            ILogger<ConsultantsModel> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // JSON Options for consistent deserialization
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        // Properties
        public List<ConsultantDto> Consultants { get; set; } = new List<ConsultantDto>();
        public ConsultantDto? ConsultantDetail { get; set; }
        public List<CourseDto> ConsultantCourses { get; set; } = new List<CourseDto>();
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }
        public PaginationInfo? CoursesPaginationInfo { get; set; }
        public ConsultantFilterDto FilterModel { get; set; } = new ConsultantFilterDto();

        // Helper properties
        public string UserName => HttpContext.Session.GetString("user_name") ?? "User";
        public string UserRole => HttpContext.Session.GetString("user_role") ?? "Member";
        public bool IsAuthenticated =>
            !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);
        public bool IsAdminUser =>
            UserRole == "Admin" || UserRole == "Manager" || UserRole == "Staff";

        public async Task<IActionResult> OnGetAsync(
            string? action = null,
            int? id = null,
            int pageIndex = 1,
            int pageSize = 12,
            int coursesPageIndex = 1,
            int coursesPageSize = 6,
            string? message = null,
            string? messageType = null,
            string? searchKeyword = null,
            string? specialty = null,
            string? status = "Active"
        )
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
            FilterModel.Specialty = specialty;
            FilterModel.Status = status;

            try
            {
                var client = GetHttpClient();

                // Load consultants list with filters
                await LoadConsultantsList(client);

                // Handle specific actions
                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadConsultantDetail(client, id.Value);
                            await LoadConsultantCourses(
                                client,
                                id.Value,
                                coursesPageIndex,
                                coursesPageSize
                            );
                        }
                        break;
                    case "specialty":
                        if (!string.IsNullOrEmpty(specialty))
                        {
                            await LoadConsultantsBySpecialty(client, specialty);
                        }
                        break;
                    case "search":
                        if (!string.IsNullOrEmpty(searchKeyword))
                        {
                            await LoadSearchResults(client, searchKeyword);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnGetAsync");
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync(string searchKeyword)
        {
            if (string.IsNullOrEmpty(searchKeyword))
            {
                return RedirectToPage(
                    "/Consultants/Consultants",
                    new { message = "Vui lòng nhập từ khóa tìm kiếm", messageType = "warning" }
                );
            }

            return RedirectToPage(
                "/Consultants/Consultants",
                new
                {
                    action = "search",
                    searchKeyword = searchKeyword,
                    message = $"Kết quả tìm kiếm cho: {searchKeyword}",
                    messageType = "info",
                }
            );
        }

        public async Task<IActionResult> OnPostFilterAsync(ConsultantFilterDto filter)
        {
            var queryParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter.SearchKeyword))
                queryParams.Add("searchKeyword", filter.SearchKeyword);
            if (!string.IsNullOrEmpty(filter.Specialty))
                queryParams.Add("specialty", filter.Specialty);
            if (!string.IsNullOrEmpty(filter.Status))
                queryParams.Add("status", filter.Status);

            queryParams.Add("pageIndex", "1");
            queryParams.Add("pageSize", filter.PageSize.ToString());

            return RedirectToPage("/Consultants/Consultants", queryParams);
        }

        public async Task<IActionResult> OnPostBookAppointmentAsync(
            int consultantId,
            DateTime appointmentDate,
            string appointmentTime,
            string notes
        )
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Request.Path });
            }

            try
            {
                var client = GetAuthenticatedClient();

                // Combine date and time
                if (!TimeSpan.TryParse(appointmentTime, out TimeSpan timeSpan))
                {
                    return RedirectToPage(
                        "/Consultants/Consultants",
                        new
                        {
                            action = "detail",
                            id = consultantId,
                            message = "Giờ hẹn không hợp lệ.",
                            messageType = "error",
                        }
                    );
                }

                var scheduledAt = appointmentDate.Date.Add(timeSpan);

                // Create form data instead of JSON
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ConsultantID", consultantId.ToString()),
                    new KeyValuePair<string, string>(
                        "ScheduledAt",
                        scheduledAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    ),
                    new KeyValuePair<string, string>("Notes", notes ?? ""),
                };

                var content = new FormUrlEncodedContent(formData);

                // Call appointment API
                var response = await client.PostAsync(
                    "https://localhost:7045/api/Appointment/create",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(
                        "Appointment created successfully: {Response}",
                        responseContent
                    );

                    return RedirectToPage(
                        "/Consultants/Consultants",
                        new
                        {
                            action = "detail",
                            id = consultantId,
                            message = "Đặt lịch hẹn thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.",
                            messageType = "success",
                        }
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Failed to book appointment. Status: {StatusCode}, Content: {Content}",
                        response.StatusCode,
                        errorContent
                    );

                    // Try to parse error response
                    string errorMessage = "Không thể đặt lịch hẹn. Vui lòng thử lại sau.";
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<dynamic>(
                            errorContent,
                            JsonOptions
                        );
                        if (errorResponse != null)
                        {
                            errorMessage = errorMessage;
                        }
                    }
                    catch
                    {
                        // Use default error message
                    }

                    return RedirectToPage(
                        "/Consultants/Consultants",
                        new
                        {
                            action = "detail",
                            id = consultantId,
                            message = errorMessage,
                            messageType = "error",
                        }
                    );
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(
                    httpEx,
                    "HTTP error booking appointment for consultant {ConsultantId}",
                    consultantId
                );
                return RedirectToPage(
                    "/Consultants/Consultants",
                    new
                    {
                        action = "detail",
                        id = consultantId,
                        message = "Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.",
                        messageType = "error",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error booking appointment for consultant {ConsultantId}",
                    consultantId
                );
                return RedirectToPage(
                    "/Consultants/Consultants",
                    new
                    {
                        action = "detail",
                        id = consultantId,
                        message = "Lỗi khi đặt lịch hẹn. Vui lòng thử lại sau.",
                        messageType = "error",
                    }
                );
            }
        }

        // Helper methods
        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            var token = HttpContext.Request.Cookies["auth_token"];

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private async Task LoadConsultantsList(HttpClient client)
        {
            try
            {
                var queryParams = new List<string>();

                if (FilterModel.PageIndex > 0)
                    queryParams.Add($"pageIndex={FilterModel.PageIndex}");
                if (FilterModel.PageSize > 0)
                    queryParams.Add($"pageSize={Math.Min(FilterModel.PageSize, 50)}");
                if (!string.IsNullOrWhiteSpace(FilterModel.SearchKeyword))
                    queryParams.Add(
                        $"searchKeyword={Uri.EscapeDataString(FilterModel.SearchKeyword.Trim())}"
                    );
                if (!string.IsNullOrWhiteSpace(FilterModel.Specialty))
                    queryParams.Add(
                        $"specialty={Uri.EscapeDataString(FilterModel.Specialty.Trim())}"
                    );
                if (!string.IsNullOrWhiteSpace(FilterModel.Status))
                    queryParams.Add($"status={FilterModel.Status}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                // Use admin endpoint if user is admin, otherwise use public endpoint
                var endpoint = IsAdminUser
                    ? $"{BASE_API_URL}/admin{queryString}"
                    : $"{BASE_API_URL}{queryString}";
                var httpClient = IsAdminUser ? GetAuthenticatedClient() : client;

                _logger.LogInformation("Calling API endpoint: {Endpoint}", endpoint);

                var response = await httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response Length: {Length}", responseContent.Length);

                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<
                            PaginatedApiResponse<ConsultantDto>
                        >(responseContent, JsonOptions);

                        if (apiResponse?.Data != null)
                        {
                            Consultants.Clear();
                            Consultants.AddRange(apiResponse.Data);
                            PaginationInfo = apiResponse.Pagination;

                            // Set ViewData for pagination with null checks
                            ViewData["CurrentPage"] = PaginationInfo?.CurrentPage ?? 1;
                            ViewData["TotalPages"] = PaginationInfo?.TotalPages ?? 1;
                            ViewData["TotalItems"] = PaginationInfo?.TotalItems ?? 0;
                            ViewData["HasPreviousPage"] = PaginationInfo?.HasPreviousPage ?? false;
                            ViewData["HasNextPage"] = PaginationInfo?.HasNextPage ?? false;

                            _logger.LogInformation(
                                "Successfully loaded {Count} consultants",
                                Consultants.Count
                            );
                        }
                        else
                        {
                            _logger.LogWarning("API response data is null");
                            Message = "Không có dữ liệu consultant.";
                            MessageType = "info";
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(
                            jsonEx,
                            "JSON deserialization error. Response content: {Content}",
                            responseContent
                        );
                        Message = "Lỗi khi xử lý dữ liệu từ máy chủ. Vui lòng thử lại sau.";
                        MessageType = "error";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "API call failed. Status: {StatusCode}, Content: {Content}",
                        response.StatusCode,
                        errorContent
                    );

                    Message = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized =>
                            "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                        System.Net.HttpStatusCode.Forbidden => "Bạn không có quyền truy cập.",
                        System.Net.HttpStatusCode.NotFound => "Không tìm thấy dữ liệu consultant.",
                        System.Net.HttpStatusCode.InternalServerError =>
                            "Lỗi máy chủ. Vui lòng thử lại sau.",
                        _ =>
                            $"Lỗi khi tải danh sách consultant. Mã lỗi: {(int)response.StatusCode}",
                    };
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when loading consultants");
                Message = "Lỗi không xác định khi tải danh sách consultant. Vui lòng thử lại sau.";
                MessageType = "error";
            }
        }

        private async Task LoadConsultantDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_API_URL}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ConsultantDto>>(
                        responseContent,
                        JsonOptions
                    );
                    ConsultantDetail = apiResponse?.Data;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to load consultant detail. Status: {StatusCode}",
                        response.StatusCode
                    );
                    Message = "Không thể tải chi tiết consultant.";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consultant detail for ID: {ConsultantId}", id);
                Message = "Lỗi khi tải chi tiết consultant.";
                MessageType = "error";
            }
        }

        private async Task LoadConsultantCourses(
            HttpClient client,
            int consultantId,
            int pageIndex,
            int pageSize
        )
        {
            try
            {
                var queryParams = new List<string>();
                if (pageIndex > 0)
                    queryParams.Add($"index={pageIndex}");
                if (pageSize > 0)
                    queryParams.Add($"pageSize={pageSize}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await client.GetAsync(
                    $"{BASE_API_URL}/{consultantId}/courses{queryString}"
                );
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<PaginatedApiResponse<CourseDto>>(
                        responseContent,
                        JsonOptions
                    );

                    if (apiResponse?.Data != null)
                    {
                        ConsultantCourses.Clear();
                        ConsultantCourses.AddRange(apiResponse.Data);
                        CoursesPaginationInfo = apiResponse.Pagination;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to load consultant courses. Status: {StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading consultant courses for ID: {ConsultantId}",
                    consultantId
                );
            }
        }

        private async Task LoadConsultantsBySpecialty(HttpClient client, string specialty)
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"specialty={Uri.EscapeDataString(specialty)}");
                if (FilterModel.PageIndex > 0)
                    queryParams.Add($"index={FilterModel.PageIndex}");
                if (FilterModel.PageSize > 0)
                    queryParams.Add($"pageSize={FilterModel.PageSize}");

                var queryString = "?" + string.Join("&", queryParams);

                var response = await client.GetAsync($"{BASE_API_URL}/specialty{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<ConsultantDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        Consultants.Clear();
                        Consultants.AddRange(apiResponse.Data);
                        PaginationInfo = apiResponse.Pagination;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to load consultants by specialty. Status: {StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading consultants by specialty: {Specialty}",
                    specialty
                );
                Message = "Lỗi khi tải consultant theo chuyên môn.";
                MessageType = "error";
            }
        }

        private async Task LoadSearchResults(HttpClient client, string searchKeyword)
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"searchKeyword={Uri.EscapeDataString(searchKeyword)}");
                if (FilterModel.PageIndex > 0)
                    queryParams.Add($"index={FilterModel.PageIndex}");
                if (FilterModel.PageSize > 0)
                    queryParams.Add($"pageSize={FilterModel.PageSize}");

                var queryString = "?" + string.Join("&", queryParams);

                var response = await client.GetAsync($"{BASE_API_URL}/search{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<ConsultantDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        Consultants.Clear();
                        Consultants.AddRange(apiResponse.Data);
                        PaginationInfo = apiResponse.Pagination;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to search consultants. Status: {StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching consultants with keyword: {SearchKeyword}",
                    searchKeyword
                );
                Message = "Lỗi khi tìm kiếm consultant.";
                MessageType = "error";
            }
        }

        // Utility methods
        public string GetUserStatusBadgeClass(string? status)
        {
            return status?.ToLower() switch
            {
                "active" => "badge bg-success",
                "inactive" => "badge bg-secondary",
                "banned" => "badge bg-danger",
                _ => "badge bg-secondary",
            };
        }

        public string GetUserStatusText(string? status)
        {
            return status?.ToLower() switch
            {
                "active" => "Hoạt động",
                "inactive" => "Không hoạt động",
                "banned" => "Bị cấm",
                _ => "Không xác định",
            };
        }

        public string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }

        public string FormatDateOnly(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public string TruncateContent(string content, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            if (content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }

        public string FormatWorkingHours(List<DateTime> workingHours)
        {
            if (workingHours == null || !workingHours.Any())
                return "Chưa cập nhật";

            return string.Join(", ", workingHours.Select(wh => wh.ToString("HH:mm")));
        }

        public bool HasValidFilters()
        {
            return !string.IsNullOrWhiteSpace(FilterModel.SearchKeyword)
                || !string.IsNullOrWhiteSpace(FilterModel.Specialty)
                || !string.IsNullOrWhiteSpace(FilterModel.Status);
        }
    }
}
