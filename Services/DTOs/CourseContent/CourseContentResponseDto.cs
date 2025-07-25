using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.CourseContent
{
    public class CourseContentResponseDto
    {
        public int ContentID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ContentType { get; set; }
        public string? ContentData { get; set; }
        public string? FileUrl { get; set; } // NEW: File URL
        public string? FileName { get; set; } // NEW: File name
        public long? FileSize { get; set; } // NEW: File size
        public string? MimeType { get; set; } // NEW: MIME type
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CourseName { get; set; }
    }

}
