using System.Text.Json;
using System.Text.Json.Serialization;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.Courses;
using DrugUserPreventionUI.Models.NewsArticles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApiConfiguration _apiConfig;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger, ApiConfiguration apiConfig)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiConfig = apiConfig;
        }

        // JSON Options for consistent deserialization
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        // Properties
        public List<NewsArticleDto> FeaturedNews { get; set; } = new List<NewsArticleDto>();
        public List<NewsArticleDto> LatestNews { get; set; } = new List<NewsArticleDto>();
        public List<CourseListHomeDto> FeaturedCourses { get; set; } =
            new List<CourseListHomeDto>();
        public List<CourseListHomeDto> PopularCourses { get; set; } = new List<CourseListHomeDto>();
        public List<CategoryHomeDTO> Categories { get; set; } = new List<CategoryHomeDTO>();
        public HomeStatsDto Stats { get; set; } = new HomeStatsDto();
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        // Helper properties
        public string UserName => HttpContext.Session.GetString("user_name") ?? "User";
        public string UserRole => HttpContext.Session.GetString("user_role") ?? "Member";
        public bool IsAuthenticated =>
            !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);

        public async Task<IActionResult> OnGetAsync(
            string? message = null,
            string? messageType = null
        )
        {
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            try
            {
                var client = GetHttpClient();

                // Load all data in parallel for better performance
                var tasks = new[]
                {
                    LoadFeaturedNews(client),
                    LoadLatestNews(client),
                    LoadFeaturedCourses(client),
                    LoadPopularCourses(client),
                    LoadCategories(client),
                    LoadStats(client),
                };

                await Task.WhenAll(tasks);

                _logger.LogInformation(
                    "Home page loaded successfully with {NewsCount} news and {CoursesCount} courses",
                    FeaturedNews.Count + LatestNews.Count,
                    FeaturedCourses.Count + PopularCourses.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");
                Message = "Có lỗi xảy ra khi tải dữ liệu trang chủ. Vui lòng thử lại sau.";
                MessageType = "error";
            }

            return Page();
        }

        // Helper methods
        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }

        private async Task LoadFeaturedNews(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync(
                    $"{_apiConfig.NewsArticlesApiUrl}?pageSize=6&newsStatus=Active"
                );
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<NewsArticleDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        FeaturedNews = apiResponse.Data.Take(6).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load featured news");
            }
        }

        private async Task LoadLatestNews(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync(
                    $"{_apiConfig.NewsArticlesApiUrl}?pageSize=4&newsStatus=Active"
                );
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<NewsArticleDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        LatestNews = apiResponse.Data.Take(4).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load latest news");
            }
        }

        private async Task LoadFeaturedCourses(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.CoursesApiUrl}?pageSize=6&isActive=true");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<CourseListHomeDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        FeaturedCourses = apiResponse.Data.Take(6).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load featured courses");
            }
        }

        private async Task LoadPopularCourses(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.CoursesApiUrl}?pageSize=4&isActive=true");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<CourseListHomeDto>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        PopularCourses = apiResponse.Data.Take(4).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load popular courses");
            }
        }

        private async Task LoadCategories(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{_apiConfig.CategoriesApiUrl}?pageSize=8");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<
                        PaginatedApiResponse<CategoryHomeDTO>
                    >(responseContent, JsonOptions);

                    if (apiResponse?.Data != null)
                    {
                        Categories = apiResponse.Data.Take(8).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load categories");
            }
        }

        private async Task LoadStats(HttpClient client)
        {
            try
            {
                // You would need to create these endpoints or calculate the stats
                Stats = new HomeStatsDto
                {
                    TotalCourses = FeaturedCourses.Count + PopularCourses.Count,
                    TotalNews = FeaturedNews.Count + LatestNews.Count,
                    TotalCategories = Categories.Count,
                    TotalUsers =
                        0 // Would need a stats endpoint
                    ,
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load stats");
            }
        }

        public string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public string TruncateContent(string content, int maxLength = 150)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Remove HTML tags
            var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]*>", "");

            if (plainText.Length <= maxLength)
                return plainText;

            var truncated = plainText.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > 0)
                truncated = truncated.Substring(0, lastSpace);

            return truncated + "...";
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

        public string GetCourseStatusBadgeClass(bool isActive)
        {
            return isActive ? "badge bg-success" : "badge bg-secondary";
        }
    }

    // Supporting DTOs
    public class HomeStatsDto
    {
        public int TotalCourses { get; set; }
        public int TotalNews { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
    }

    public class CourseListHomeDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = "";
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int EnrollmentCount { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
    }

    public class CategoryHomeDTO
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
