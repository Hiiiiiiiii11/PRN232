using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.ConsultantUser
{
    public class ConsultantStatsDto
    {
        public int TotalConsultants { get; set; }
        public int ActiveConsultants { get; set; }
        public int InactiveConsultants { get; set; }
        public int TotalCourses { get; set; }
        public List<ConsultantDto> RecentConsultants { get; set; } = new List<ConsultantDto>();
    }
}
