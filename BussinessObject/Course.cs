using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    /// <summary>
    /// Tạo, quản lý thông tin khóa học
    /// </summary>
    public class Course
    {
        [Key]
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetGroup { get; set; }
        public string AgeGroup { get; set; }
        public string ContentURL { get; set; }
        public string ThumbnailURL { get; set; } // URL của hình ảnh đại diện khóa học
        public DateTime CreatedAt { get; set; }
        public bool isActive { get; set; } = true;
        public bool isAccept { get; set; } = true;
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }
        public virtual ICollection<CourseRegistration>? Registrations { get; set; }
        public virtual ICollection<CourseContent>? Contents { get; set; }
    }
}
