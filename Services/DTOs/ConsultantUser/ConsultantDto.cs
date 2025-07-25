using Services.DTOs.Courses;
using Services.DTOs.User;

namespace Services.DTOs.ConsultantUser
{
    public class ConsultantDto
    {
        public int ConsultantID { get; set; }
        public string Qualifications { get; set; }
        public string Specialty { get; set; }
        public List<DateTime> WorkingHours { get; set; }

        // User information
        public virtual UserDTO User { get; set; }

        // Courses created by this consultant
        public virtual List<CourseDto> CreatedCourses { get; set; } = new List<CourseDto>();
    }
}
