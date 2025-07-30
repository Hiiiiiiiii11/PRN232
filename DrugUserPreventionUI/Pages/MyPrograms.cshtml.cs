using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using DrugUserPreventionUI.Models.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using DrugUserPreventionUI.Pages.ProgramDetails;

namespace DrugUserPreventionUI.Pages.MyPrograms
{
    public class MyProgramsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string API_BASE_URL = "https://localhost:7045";

        public MyProgramsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UserProgramModel> UserPrograms { get; set; } = new();
        public ProgramFilterModel FilterForm { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(
            string? search = null,
            string? status = null,
            int page = 1)
        {
            // Check if user is logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/MyPrograms" });
            }

            // Set filter parameters
            FilterForm.SearchKeyword = search;
            FilterForm.Status = status;
            CurrentPage = page;

            try
            {
                await LoadUserProgramsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra khi tải danh sách chương trình: " + ex.Message;
                return Page();
            }
        }

        private async Task LoadUserProgramsAsync()
        {
            ErrorMessage = null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = HttpContext.Request.Cookies["auth_token"];
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    ErrorMessage = "Không thể xác định người dùng.";
                    return;
                }

                // Gửi yêu cầu GET
                var request = new HttpRequestMessage(HttpMethod.Get, $"{API_BASE_URL}/api/ProgramParticipation/user/{userId}");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var programList = JsonSerializer.Deserialize<List<UserProgramModel>>(json, options);

                    if (programList != null)
                    {
                        UserPrograms = programList;
                        TotalItems = programList.Count;
                        TotalPages = 1;
                        CurrentPage = 1;
                    }
                }
                else
                {
                    ErrorMessage = $"Lỗi API: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi gọi API: {ex.Message}";
            }
        }
        public async Task<IActionResult> OnPostCancelParticipationAsync(int participationId)
        {
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login", new { returnUrl = "/MyPrograms" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{API_BASE_URL}/api/ProgramParticipation/{participationId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đã hủy tham gia chương trình thành công.";
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Lỗi khi hủy tham gia: {err}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
            }

            // Load lại danh sách
            await LoadUserProgramsAsync();
            return Page();
        }


        public int GetCurrentUserId()
        {
            var token = HttpContext.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var userIdClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                    return userId;
            }
            return 0;
        }

    }

    // Models for the page
    public class UserProgramModel
    {
        public int ParticipationID { get; set; }
        public int? UserID { get; set; }
        public string? UserFullName { get; set; }
        public int? ProgramID { get; set; }
        public string? ProgramTitle { get; set; }
        public DateTime ParticipatedAt { get; set; }

        // Thêm property để ánh xạ từ ProgramDetailDto
        public ProgramDetailModel? Program { get; set; }

        // Convenience properties để bind dễ hơn trong Razor view
        public string? ProgramName => Program?.Title;
        public string? Description => Program?.Description;
        public string? ImageUrl => Program?.ThumbnailURL;
        public DateTime StartDate => Program?.StartDate ?? DateTime.MinValue;
        public DateTime EndDate => Program?.EndDate ?? DateTime.MinValue;
        public string? Location => Program?.Location;
    }

    public class ProgramDetailModel
    {
        public int ProgramID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailURL { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }



    public class ProgramFilterModel
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
    }

    public class PaginatedApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<T>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}