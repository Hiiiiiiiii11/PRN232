using System.Text.Json;
using System.Text.Json.Serialization;
using DrugUserPreventionUI.Models.Categories;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.NewsArticles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages.NewsArticles
{
    public class NewsArticleModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NewsArticleModel> _logger;

        public NewsArticleModel(
            IHttpClientFactory httpClientFactory,
            ILogger<NewsArticleModel> logger
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
        public List<NewsArticleDto> NewsArticles { get; set; } = new List<NewsArticleDto>();
        public NewsArticleDto? NewsDetail { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
        public PaginationInfo? PaginationInfo { get; set; }
        public NewsArticleFilterDto FilterModel { get; set; } = new NewsArticleFilterDto();

        // Add Categories for dropdown
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

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
            string? message = null,
            string? messageType = null,
            string? searchKeyword = null,
            int? categoryId = null,
            string? newsSource = null,
            string? newsStatus = "Active",
            DateTime? fromDate = null,
            DateTime? toDate = null
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
            FilterModel.CategoryID = categoryId;
            FilterModel.NewsSource = newsSource;
            FilterModel.NewsStatus = newsStatus;
            FilterModel.FromDate = fromDate;
            FilterModel.ToDate = toDate;

            try
            {
                var client = GetHttpClient();

                // Load dropdown data first
                await LoadDropdownData(client);

                // Load news articles with filters
                await LoadNewsArticlesList(client);

                // Handle specific actions
                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadNewsDetail(client, id.Value);
                        }
                        break;
                    case "category":
                        if (id.HasValue)
                        {
                            await LoadNewsByCategory(client, id.Value);
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
                    "/NewsArticles/NewsArticles",
                    new { message = "Vui lòng nhập từ khóa tìm kiếm", messageType = "warning" }
                );
            }

            return RedirectToPage(
                "/NewsArticles/NewsArticles",
                new
                {
                    action = "search",
                    searchKeyword = searchKeyword,
                    message = $"Kết quả tìm kiếm cho: {searchKeyword}",
                    messageType = "info",
                }
            );
        }

        public async Task<IActionResult> OnPostFilterAsync(NewsArticleFilterDto filter)
        {
            var queryParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter.SearchKeyword))
                queryParams.Add("searchKeyword", filter.SearchKeyword);
            if (filter.CategoryID.HasValue)
                queryParams.Add("categoryId", filter.CategoryID.Value.ToString());
            if (!string.IsNullOrEmpty(filter.NewsSource))
                queryParams.Add("newsSource", filter.NewsSource);
            if (!string.IsNullOrEmpty(filter.NewsStatus))
                queryParams.Add("newsStatus", filter.NewsStatus);
            if (filter.FromDate.HasValue)
                queryParams.Add("fromDate", filter.FromDate.Value.ToString("yyyy-MM-dd"));
            if (filter.ToDate.HasValue)
                queryParams.Add("toDate", filter.ToDate.Value.ToString("yyyy-MM-dd"));

            queryParams.Add("pageIndex", "1");
            queryParams.Add("pageSize", filter.PageSize.ToString());

            return RedirectToPage("/NewsArticles/NewsArticles", queryParams);
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

        // NEW METHOD: Load dropdown data
        private async Task LoadDropdownData(HttpClient client)
        {
            try
            {
                // Load Categories
                var categoriesResponse = await client.GetAsync(
                    $"{ApiUrlHelper.GetCategoriesApiUrl()}?pageSize=100"
                );
                if (categoriesResponse.IsSuccessStatusCode)
                {
                    var responseContent = await categoriesResponse.Content.ReadAsStringAsync();
                    var categoriesApiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<CategoryDTO>
                    >(responseContent, JsonOptions);

                    if (categoriesApiResponse?.Data != null)
                    {
                        Categories = categoriesApiResponse.Data.ToList();
                        _logger.LogInformation("Loaded {Count} categories", Categories.Count);
                    }
                }

                // Load News Sources (get unique sources from existing news)
                var sourcesResponse = await client.GetAsync($"{ApiUrlHelper.GetNewsArticlesApiUrl()}?pageSize=1000");
                if (sourcesResponse.IsSuccessStatusCode)
                {
                    var responseContent = await sourcesResponse.Content.ReadAsStringAsync();
                    var newsApiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<NewsArticleDto>
                    >(responseContent, JsonOptions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load dropdown data");

                // Provide fallback data
                Categories = new List<CategoryDTO>
                {
                    new() { CategoryID = 1, CategoryName = "Phòng chống tệ nạn" },
                    new() { CategoryID = 2, CategoryName = "Sức khỏe cộng đồng" },
                    new() { CategoryID = 3, CategoryName = "Giáo dục - Tuyên truyền" },
                    new() { CategoryID = 4, CategoryName = "Chính sách - Pháp luật" },
                };
            }
        }

        private async Task LoadNewsArticlesList(HttpClient client)
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
                if (FilterModel.CategoryID.HasValue && FilterModel.CategoryID.Value > 0)
                    queryParams.Add($"categoryId={FilterModel.CategoryID.Value}");
                if (!string.IsNullOrWhiteSpace(FilterModel.NewsSource))
                    queryParams.Add(
                        $"newsSource={Uri.EscapeDataString(FilterModel.NewsSource.Trim())}"
                    );
                if (!string.IsNullOrWhiteSpace(FilterModel.NewsStatus))
                    queryParams.Add($"newsStatus={FilterModel.NewsStatus}");
                if (FilterModel.FromDate.HasValue)
                    queryParams.Add($"fromDate={FilterModel.FromDate.Value:yyyy-MM-dd}");
                if (FilterModel.ToDate.HasValue)
                    queryParams.Add($"toDate={FilterModel.ToDate.Value:yyyy-MM-dd}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                // Use admin endpoint if user is admin, otherwise use public endpoint
                var endpoint = IsAdminUser
                    ? $"{ApiUrlHelper.GetNewsArticlesApiUrl()}/admin{queryString}"
                    : $"{ApiUrlHelper.GetNewsArticlesApiUrl()}{queryString}";
                var httpClient = IsAdminUser ? GetAuthenticatedClient() : client;

                _logger.LogInformation("Calling API endpoint: {Endpoint}", endpoint);

                var response = await httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response Length: {Length}", responseContent.Length);

                    if (responseContent.Length > 0)
                    {
                        var preview =
                            responseContent.Length > 500
                                ? responseContent.Substring(0, 500) + "..."
                                : responseContent;
                        _logger.LogDebug("API Response Preview: {Preview}", preview);
                    }

                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<
                            PaginatedApiResponse<NewsArticleDto>
                        >(responseContent, JsonOptions);

                        if (apiResponse?.Data != null)
                        {
                            NewsArticles.Clear();
                            NewsArticles.AddRange(apiResponse.Data);
                            PaginationInfo = apiResponse.Pagination;

                            // Set ViewData for pagination with null checks
                            ViewData["CurrentPage"] = PaginationInfo?.CurrentPage ?? 1;
                            ViewData["TotalPages"] = PaginationInfo?.TotalPages ?? 1;
                            ViewData["TotalItems"] = PaginationInfo?.TotalItems ?? 0;
                            ViewData["HasPreviousPage"] = PaginationInfo?.HasPreviousPage ?? false;
                            ViewData["HasNextPage"] = PaginationInfo?.HasNextPage ?? false;

                            _logger.LogInformation(
                                "Successfully loaded {Count} news articles",
                                NewsArticles.Count
                            );
                        }
                        else
                        {
                            _logger.LogWarning("API response data is null");
                            Message = "Không có dữ liệu tin tức.";
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

                        try
                        {
                            var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                                responseContent,
                                JsonOptions
                            );
                            Message = errorResponse?.Message ?? "Lỗi khi xử lý dữ liệu tin tức.";
                            MessageType = "error";
                        }
                        catch
                        {
                            Message = "Lỗi khi xử lý dữ liệu từ máy chủ. Vui lòng thử lại sau.";
                            MessageType = "error";
                        }
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
                        System.Net.HttpStatusCode.NotFound => "Không tìm thấy dữ liệu tin tức.",
                        System.Net.HttpStatusCode.InternalServerError =>
                            "Lỗi máy chủ. Vui lòng thử lại sau.",
                        _ => $"Lỗi khi tải tin tức. Mã lỗi: {(int)response.StatusCode}",
                    };
                    MessageType = "error";
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error when loading news articles");
                Message = "Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.";
                MessageType = "error";
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                _logger.LogError(tcEx, "Request timeout when loading news articles");
                Message = "Yêu cầu bị timeout. Vui lòng thử lại.";
                MessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when loading news articles");
                Message = "Lỗi không xác định khi tải tin tức. Vui lòng thử lại sau.";
                MessageType = "error";
            }
        }

        private async Task LoadNewsDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{ApiUrlHelper.GetNewsArticlesApiUrl()}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<NewsArticleDto>>(
                        responseContent,
                        JsonOptions
                    );
                    NewsDetail = apiResponse?.Data;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to load news detail. Status: {StatusCode}",
                        response.StatusCode
                    );
                    Message = "Không thể tải chi tiết bài viết.";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading news detail for ID: {NewsId}", id);
                Message = "Lỗi khi tải chi tiết bài viết.";
                MessageType = "error";
            }
        }

        private async Task LoadNewsByCategory(HttpClient client, int categoryId)
        {
            try
            {
                var queryParams = new List<string>();
                if (FilterModel.PageIndex > 0)
                    queryParams.Add($"pageIndex={FilterModel.PageIndex}");
                if (FilterModel.PageSize > 0)
                    queryParams.Add($"pageSize={FilterModel.PageSize}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await client.GetAsync(
                    $"{ApiUrlHelper.GetNewsArticlesApiUrl()}/category/{categoryId}{queryString}"
                );
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<NewsArticleDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        NewsArticles.Clear();
                        NewsArticles.AddRange(apiResponse.Data);
                        PaginationInfo = apiResponse.Pagination;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to load news by category. Status: {StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading news by category: {CategoryId}", categoryId);
                Message = "Lỗi khi tải tin tức theo danh mục.";
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
                    queryParams.Add($"pageIndex={FilterModel.PageIndex}");
                if (FilterModel.PageSize > 0)
                    queryParams.Add($"pageSize={FilterModel.PageSize}");

                var queryString = "?" + string.Join("&", queryParams);

                var response = await client.GetAsync($"{ApiUrlHelper.GetNewsArticlesApiUrl()}/search{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<NewsArticleDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        NewsArticles.Clear();
                        NewsArticles.AddRange(apiResponse.Data);
                        PaginationInfo = apiResponse.Pagination;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to search news. Status: {StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching news with keyword: {SearchKeyword}",
                    searchKeyword
                );
                Message = "Lỗi khi tìm kiếm tin tức.";
                MessageType = "error";
            }
        }

        public string GetNewsStatusBadgeClass(string? status)
        {
            return status?.ToLower() switch
            {
                "active" => "badge bg-success",
                "inactive" => "badge bg-secondary",
                "draft" => "badge bg-warning",
                _ => "badge bg-secondary",
            };
        }

        public string GetNewsStatusText(string? status)
        {
            return status?.ToLower() switch
            {
                "active" => "Hoạt động",
                "inactive" => "Không hoạt động",
                "draft" => "Bản nháp",
                _ => "Không xác định",
            };
        }

        public string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }

        public string TruncateContent(string content, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            if (content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }

        public bool HasValidFilters()
        {
            return !string.IsNullOrWhiteSpace(FilterModel.SearchKeyword)
                || FilterModel.CategoryID.HasValue
                || !string.IsNullOrWhiteSpace(FilterModel.NewsSource)
                || !string.IsNullOrWhiteSpace(FilterModel.NewsStatus)
                || FilterModel.FromDate.HasValue
                || FilterModel.ToDate.HasValue;
        }
    }
}
