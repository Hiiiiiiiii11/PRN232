using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Courses
{
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Tiêu đề khóa học không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả khóa học không được để trống")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Nhóm đối tượng không được để trống")]
        [MaxLength(100, ErrorMessage = "Nhóm đối tượng không được vượt quá 100 ký tự")]
        public string TargetGroup { get; set; }

        [Required(ErrorMessage = "Nhóm tuổi không được để trống")]
        [MaxLength(50, ErrorMessage = "Nhóm tuổi không được vượt quá 50 ký tự")]
        public string AgeGroup { get; set; }

        [Url(ErrorMessage = "URL nội dung không hợp lệ")]
        public string? ContentURL { get; set; }
        ///// Danh sách kỹ năng được phát triển qua khóa học
        //public List<string> Skills { get; set; } = new();
        public string ThumbnailURL { get; set; } // URL của hình ảnh đại diện khóa học

    }
}
