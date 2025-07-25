using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugUserPreventionUI.Models.CourseDashboard
{
    public class CourseResponseDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        // Skills and ThumbnailUrl removed temporarily
        public string? ContentURL { get; set; }
        public string ThumbnailURL { get; set; } // URL của hình ảnh đại diện khóa học
        public int? CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }
}
