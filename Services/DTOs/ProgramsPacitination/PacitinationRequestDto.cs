using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.ProgramsPacitination
{
    public class ProgramParticipationCreateRequest
    {
        public int UserID { get; set; }
        public int ProgramID { get; set; }
    }

}
