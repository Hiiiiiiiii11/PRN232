using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.CourseContent
{
    public class CreateCourseContentDto
    {
        [Required(ErrorMessage = "ID khóa học không được để trống")]
        public int CourseID { get; set; }

        [Required(ErrorMessage = "Tiêu đề nội dung không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại nội dung không được để trống")]
        [MaxLength(50, ErrorMessage = "Loại nội dung không được vượt quá 50 ký tự")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string ContentData { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn 0")]
        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
