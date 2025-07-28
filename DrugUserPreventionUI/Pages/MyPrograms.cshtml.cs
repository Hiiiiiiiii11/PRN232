using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using DrugUserPreventionUI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace DrugUserPreventionUI.Pages
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
            var token = HttpContext.Session.GetString("JWTToken");
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
            // TODO: Implement API for Programs when backend is ready
            // For now, return empty list to prevent errors
            UserPrograms = new List<UserProgramModel>();
            CurrentPage = 1;
            TotalPages = 1;
            TotalItems = 0;
            
            // Optional: Add info message that this feature is under development
            ErrorMessage = null; // Clear any previous errors
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }

    // Models for the page
    public class UserProgramModel
    {
        public int ProgramID { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ParticipationStatus { get; set; } = string.Empty;
        public double? Progress { get; set; }
        public DateTime ParticipationDate { get; set; }
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