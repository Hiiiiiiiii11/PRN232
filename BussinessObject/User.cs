using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string? AvatarUrl { get; set; } // URL đến ảnh đại diện của người dùng
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // Male, Female, Other
        public string Role { get; set; } = "Guest";
        public string Status { get; set; } = "Active"; // Active, Inactive, Banned
        public DateTime CreatedAt { get; set; }

        public string? VerificationToken { get; set; } // Token xác thực email
        public bool IsEmailVerified { get; set; } = false; // Trạng thái xác thực email

        // Thuộc tính mới cho chức năng quên mật khẩu
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public virtual Consultant? ConsultantProfile { get; set; }

        public virtual ICollection<Course> CreatedCourses { get; set; }
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public virtual ICollection<UserSurveyResponse> SurveyResponses { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<ProgramParticipation> Participations { get; set; }
    }
}
