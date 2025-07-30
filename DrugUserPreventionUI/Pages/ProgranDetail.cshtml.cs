using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using DrugUserPreventionUI.Models.Common;

namespace DrugUserPreventionUI.Pages.ProgramDetails
{
    public class ProgramDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string API_BASE_URL = "https://localhost:7045";

        public ProgramDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ProgramDetailDto Program { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{API_BASE_URL}/api/Programs/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Program = JsonSerializer.Deserialize<ProgramDetailDto>(json, options);

            if (Program == null)
            {
                return NotFound();
            }

            return Page();
        }
    }

    public class ProgramDetailDto
    {
        public int ProgramID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailURL { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
