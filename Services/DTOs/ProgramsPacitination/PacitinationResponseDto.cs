using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.ProgramsPacitination
{
    public class ProgramParticipationResponse
    {
        public int ParticipationID { get; set; }
        public int? UserID { get; set; }
        public string? UserFullName { get; set; }

        public int? ProgramID { get; set; }
        public string? ProgramTitle { get; set; }

        public DateTime ParticipatedAt { get; set; }
        public ProgramDetailDto Program { get; set; } = new ProgramDetailDto();
    }

}
