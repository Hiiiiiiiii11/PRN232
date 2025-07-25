using Services.DTOs.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Dashboard
{
    public class CourseEnrollmentStatsDto
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int InProgressEnrollments { get; set; }
        public double CompletionRate { get; set; }
        public double AverageProgress { get; set; }
        public List<RegistrationListDto> RecentEnrollments { get; set; } = new();
    }
}
