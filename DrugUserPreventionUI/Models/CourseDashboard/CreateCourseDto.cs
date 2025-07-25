using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugUserPreventionUI.Models.CourseDashboard
{
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả khóa học là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhóm đối tượng là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Nhóm đối tượng không được vượt quá 100 ký tự")]
        public string TargetGroup { get; set; } = string.Empty;

        [Required(ErrorMessage = "Độ tuổi là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Độ tuổi không được vượt quá 50 ký tự")]
        public string AgeGroup { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL nội dung không hợp lệ")]
        public string? ContentURL { get; set; }

        [Url(ErrorMessage = "URL hình đại diện không hợp lệ")]
        public string? ThumbnailURL { get; set; }
    }
}
