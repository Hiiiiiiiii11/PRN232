using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.Consultants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DrugUserPreventionUI.Pages.Consultants
{
    public class ConsultantCalendarModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConsultantCalendarModel> _logger;
        private const string BASE_API_URL = "https://localhost:7045/api";

        public ConsultantCalendarModel(IHttpClientFactory httpClientFactory, ILogger<ConsultantCalendarModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Properties
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; } = "";
        public DateTime WeekStartDate { get; set; }
        public WeeklyCalendarDto? WeeklyCalendar { get; set; }
        public string CurrentWeekDisplay { get; set; } = "";
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        // Helper properties
        public bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);

        public async Task<IActionResult> OnGetAsync(int consultantId, DateTime? weekStart = null, string? message = null, string? messageType = null)
        {
            ConsultantId = consultantId;
            WeekStartDate = weekStart ?? GetStartOfWeek(DateTime.Today);
            Message = message;
            MessageType = messageType;

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Load consultant info
                await LoadConsultantInfo(client);

                // Load weekly calendar
                await LoadWeeklyCalendar(client);

                // Set display text
                var weekEnd = WeekStartDate.AddDays(6);
                CurrentWeekDisplay = $"{WeekStartDate:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consultant calendar");
                Message = "Có lỗi xảy ra khi tải lịch tư vấn.";
                MessageType = "error";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostBookAppointmentAsync(int consultantId, DateTime slotDateTime, string? notes = null)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login", new { returnUrl = $"/consultants/{consultantId}/calendar" });
            }

            try
            {
                var client = GetAuthenticatedClient();

                var appointmentRequest = new
                {
                    ConsultantID = consultantId,
                    ScheduledAt = slotDateTime,
                    Notes = notes ?? ""
                };

                var content = new StringContent(JsonSerializer.Serialize(appointmentRequest), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Appointment/create", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Consultants/ConsultantCalendar", new
                    {
                        consultantId,
                        weekStart = WeekStartDate.ToString("yyyy-MM-dd"),
                        message = "Đặt lịch thành công! Chúng tôi sẽ liên hệ với bạn sớm.",
                        messageType = "success"
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Consultants/ConsultantCalendar", new
                    {
                        consultantId,
                        weekStart = WeekStartDate.ToString("yyyy-MM-dd"),
                        message = "Không thể đặt lịch. Thời gian đã chọn có thể đã được đặt.",
                        messageType = "error"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment");
                return RedirectToPage("/Consultants/ConsultantCalendar", new
                {
                    consultantId,
                    weekStart = WeekStartDate.ToString("yyyy-MM-dd"),
                    message = "Có lỗi xảy ra khi đặt lịch.",
                    messageType = "error"
                });
            }
        }

        private async Task LoadConsultantInfo(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_API_URL}/ConsultantUser/{ConsultantId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ConsultantDto>>(responseContent, GetJsonOptions());
                    
                    if (apiResponse?.Data?.User != null)
                    {
                        ConsultantName = apiResponse.Data.User.FullName ?? "Chuyên gia tư vấn";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consultant info");
                ConsultantName = "Chuyên gia tư vấn";
            }
        }

        private async Task LoadWeeklyCalendar(HttpClient client)
        {
            try
            {
                var weekStartParam = WeekStartDate.ToString("yyyy-MM-dd");
                var response = await client.GetAsync($"{BASE_API_URL}/ConsultantCalendar/{ConsultantId}/calendar/weekly?weekStartDate={weekStartParam}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<WeeklyCalendarDto>>(responseContent, GetJsonOptions());
                    WeeklyCalendar = apiResponse?.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to load weekly calendar. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weekly calendar");
            }
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Request.Cookies["auth_token"];

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            return date.AddDays(-dayOfWeek).Date;
        }

        public string GetDayName(int dayIndex)
        {
            return dayIndex switch
            {
                0 => "CN",
                1 => "T2", 
                2 => "T3",
                3 => "T4",
                4 => "T5",
                5 => "T6",
                6 => "T7",
                _ => ""
            };
        }
    }

    // DTOs for the calendar
    public class WeeklyCalendarDto
    {
        public int ConsultantID { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<ConsultantCalendarDto> DailyCalendars { get; set; } = new List<ConsultantCalendarDto>();
        public List<ConsultantScheduleDto> WeeklySchedule { get; set; } = new List<ConsultantScheduleDto>();
    }

    public class ConsultantCalendarDto
    {
        public int ConsultantID { get; set; }
        public DateTime Date { get; set; }
        public List<AppointmentSlotDto> AvailableSlots { get; set; } = new List<AppointmentSlotDto>();
        public List<AppointmentSlotDto> BookedSlots { get; set; } = new List<AppointmentSlotDto>();
        public bool HasAvailability { get; set; }
    }

    public class AppointmentSlotDto
    {
        public int SlotID { get; set; }
        public int ConsultantID { get; set; }
        public DateTime SlotDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = "";
        public int? AppointmentID { get; set; }
        public bool IsAvailable => Status == "Available";
        public string FormattedTime => SlotDateTime.ToString("HH:mm");
        public string FormattedDate => SlotDateTime.ToString("dd/MM/yyyy");
    }

    public class ConsultantScheduleDto
    {
        public int ScheduleID { get; set; }
        public int ConsultantID { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}