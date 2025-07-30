using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using DrugUserPreventionUI.Models;

namespace DrugUserPreventionUI.Pages.ManageProgram
{
    public class ManageProgramsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string API_BASE_URL = "https://localhost:7045";
        public List<ProgramDto> Programs { get; set; } = new();

        [BindProperty]
        public ProgramDto Input { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public ManageProgramsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient("Api");
            Programs = await client.GetFromJsonAsync<List<ProgramDto>>($"{API_BASE_URL}/api/Programs") ?? new();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var client = _clientFactory.CreateClient("Api");

            // Lấy UserID từ JWT cookie
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                ErrorMessage = "Không xác định được người dùng.";
                return RedirectToPage("/ManageProgram/ManagePrograms");
            }

            Input.CreatedBy = userId;

            HttpResponseMessage response;
            if (Input.ProgramID > 0)
                response = await client.PutAsJsonAsync($"{API_BASE_URL}/api/Programs/{Input.ProgramID}", Input);
            else
                response = await client.PostAsJsonAsync($"{API_BASE_URL}/api/Programs", Input);

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Lưu thành công!";
            else
                ErrorMessage = "Có lỗi xảy ra.";

            return RedirectToPage("/ManageProgram/ManagePrograms");
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _clientFactory.CreateClient("Api");
            var response = await client.DeleteAsync($"{API_BASE_URL}/api/Programs/{id}");

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Xóa chương trình thành công!";
            else
                ErrorMessage = "Không thể xóa chương trình.";

            return RedirectToPage("/ManageProgram/ManageProgram");
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

    public class ProgramDto
    {
        public int ProgramID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = "";
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
    }
}
