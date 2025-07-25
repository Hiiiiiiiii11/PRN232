using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Courses
{
    public class CreateRegistrationDto
    {
        [Required(ErrorMessage = "ID khóa học không được để trống")]
        public int CourseID { get; set; }
    }
    public class CourseRegistrationResponseDto
    {
        public int RegistrationID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string CourseTitle { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool Completed { get; set; }
    }
}
