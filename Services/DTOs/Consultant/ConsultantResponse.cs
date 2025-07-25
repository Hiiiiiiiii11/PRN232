using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Consultant
{
    public class ConsultantResponse
    {
        public int ConsultantID { get; set; }
        public string Qualifications { get; set; }
        public string Specialty { get; set; }
        public List<DateTime> WorkingHours { get; set; }
    }
}
