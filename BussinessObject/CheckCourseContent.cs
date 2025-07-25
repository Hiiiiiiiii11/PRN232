using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects
{
    /// <summary>
    /// Ghi nhận trạng thái người học đã hoàn thành nội dung nào trong khóa học.
    /// </summary>
    public class CheckCourseContent
    {
        [Key]
        public int CheckID { get; set; }

        [Required]
        public int? RegistrationID { get; set; }

        [Required]
        public int? ContentID { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [ForeignKey("RegistrationID")]
        public virtual CourseRegistration? Registration { get; set; }

        [ForeignKey("ContentID")]
        public virtual CourseContent? Content { get; set; }
    }
}
