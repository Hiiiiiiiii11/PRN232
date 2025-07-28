using System.ComponentModel.DataAnnotations;

namespace DrugUserPreventionUI.Models.Common
{
    public class ProfileForm
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }
    }
} 