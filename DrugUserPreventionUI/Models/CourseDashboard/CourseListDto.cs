using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugUserPreventionUI.Models.CourseDashboard
{
    public class CourseListDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetGroup { get; set; }
        public string AgeGroup { get; set; }
        //public List<string> Skills { get; set; } = new(); // NEW: Skills list
        public string? ThumbnailUrl { get; set; } // NEW: Thumbnail URL
        public string CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }
}
