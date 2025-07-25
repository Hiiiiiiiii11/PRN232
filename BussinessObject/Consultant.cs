using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Consultant
    {
        [Key]
        [ForeignKey("User")]
        public int ConsultantID { get; set; }
        public string Qualifications { get; set; }
        public string Specialty { get; set; }
        public List<DateTime> WorkingHours { get; set; }

        public virtual User User { get; set; }
    }
}
