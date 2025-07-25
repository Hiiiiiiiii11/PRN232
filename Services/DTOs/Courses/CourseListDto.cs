using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Courses
{
    public class CourseListDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetGroup { get; set; }
        public string AgeGroup { get; set; }
        public string ThumbnailURL { get; set; } // URL của hình ảnh đại diện khóa học
        public string CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }
}
