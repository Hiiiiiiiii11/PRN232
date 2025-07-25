using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.ConsultantUser
{
    public class ConsultantFilterDto : BasePaginationDto
    {
        public string? SearchKeyword { get; set; }
        public string? Specialty { get; set; }
        public string? Status { get; set; } = "Active";
    }
}
