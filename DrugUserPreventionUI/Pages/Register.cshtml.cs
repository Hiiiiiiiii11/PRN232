using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DrugUserPreventionUI.Configuration;

namespace DrugUserPreventionUI.Pages
{
    public class RegisterModel : PageModel
    {
        public RegisterModel()
        {
        }

        [BindProperty]
        public RegisterRequest RegisterForm { get; set; } = new();

        public string? Message { get; set; }
        public string MessageType { get; set; } = "success";

        public async Task<IActionResult> OnPostAsync()
        {
            using var client = new HttpClient();
            var query =
                $"?FullName={RegisterForm.FullName}&Email={RegisterForm.Email}&Username={RegisterForm.Username}"
                + $"&Password={RegisterForm.Password}&Phone={RegisterForm.Phone}"
                + $"&DateOfBirth={RegisterForm.DateOfBirth:MM/dd/yyyy}&Gender={RegisterForm.Gender}&Role=Member";

            try
            {
                var response = await client.PostAsync(
                    $"{ApiUrlHelper.GetAuthUrl()}/register{query}",
                    null
                );
                if (response.IsSuccessStatusCode)
                {
                    Message = "Đăng ký thành công! Bạn có thể đăng nhập.";
                    MessageType = "success";
                    return Page();
                }
                else
                {
                    Message = "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.";
                    MessageType = "danger";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi hệ thống: {ex.Message}";
                MessageType = "danger";
                return Page();
            }
        }
    }

    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "Male";
        public string Role { get; set; } = "Member";
    }
}
