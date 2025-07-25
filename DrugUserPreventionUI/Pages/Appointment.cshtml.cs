using DrugUserPreventionUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;

namespace DrugUserPreventionUI.Pages
{
    public class AppointmentModel : PageModel
    {
        public IHttpClientFactory HttpClientFactory;
        public AppointmentModel(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public List<AppointmentDTO> appointments { get; set; } = new List<AppointmentDTO>();
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var cliente = HttpClientFactory.CreateClient();

                var httpResponseMessager = await cliente.GetAsync("https://localhost:7045/api/Appointment");
                httpResponseMessager.EnsureSuccessStatusCode();
                var apiResponse = await httpResponseMessager.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<AppointmentDTO>>>();

                appointments.AddRange(apiResponse.Data);

            }
            catch (Exception ex)
            {

            }

            return Page();
        }
    }
}
