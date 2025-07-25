using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Registration
{
    public class RegistrationListDto
    {
        public int RegistrationID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string TargetGroup { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool Completed { get; set; }
        public double Progress { get; set; }
    }
}
