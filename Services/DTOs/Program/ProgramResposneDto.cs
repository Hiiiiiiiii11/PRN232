using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Program
{
    public class ProgramResponseDto
    {
        public int ProgramID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailURL { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
