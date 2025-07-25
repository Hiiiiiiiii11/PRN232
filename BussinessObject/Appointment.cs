using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }
        public int? UserID { get; set; }
        public int? ConsultantID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Completed, Cancelled
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; } = null!;
    }
}
