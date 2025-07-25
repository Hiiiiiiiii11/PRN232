using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Program
    {
        [Key]
        public int ProgramID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailURL { get; set; } // URL of the program's thumbnail image
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual User Creator { get; set; }
        public virtual ICollection<ProgramParticipation> Participants { get; set; }
    }
}
