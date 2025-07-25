using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class CourseContent
    {
        [Key]
        public int ContentID { get; set; }

        public int? CourseID { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } // Ví dụ: "Chương 1: Giới thiệu"

        public string? Description { get; set; } // Mô tả nội dung, bài học

        public string? ContentType { get; set; } // Ví dụ: "Video", "Text", "Quiz"

        public string? ContentData { get; set; } // URL video, nội dung HTML/text, link tài liệu

        public int OrderIndex { get; set; } // Thứ tự hiển thị

        public bool isActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public virtual ICollection<CheckCourseContent>? ProgressByUsers { get; set; }
    }
}
