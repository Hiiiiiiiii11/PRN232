using Services.DTOs.Courses;
using Services.DTOs.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Dashboard
{
    public class UserLearningDashboardDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalRegistrations { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public double OverallProgress { get; set; }
        public List<RegistrationListDto> RecentRegistrations { get; set; } = new();
        public List<RegistrationListDto> InProgressCoursesList { get; set; } = new();
    }
}
