using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class ProgramParticipation
    {
        [Key]
        public int ParticipationID { get; set; }
        public int? UserID { get; set; }
        public int? ProgramID { get; set; }
        public DateTime ParticipatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Program Program { get; set; }
    }
}
