using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    /// <summary>
    /// Ghi nhận thông tin đăng ký học của người dùng cho khóa học cụ thể.
    /// </summary>
    public class CourseRegistration
    {
        [Key]
        public int RegistrationID { get; set; }
        public int? UserID { get; set; }
        public int? CourseID { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool Completed { get; set; }

        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
        public virtual ICollection<CheckCourseContent>? ContentProgress { get; set; }
    }
}
