using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Registration
{
    public class RegistrationFilterDto : BasePaginationDto
    {
        public int? UserID { get; set; }
        public int? CourseID { get; set; }
        public bool? Completed { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? MinProgress { get; set; }
        public double? MaxProgress { get; set; }
    }
}
