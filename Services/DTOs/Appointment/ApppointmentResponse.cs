using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Appointment
{
    public class ApppointmentResponse
    {
        public int AppointmentID { get; set; }
        public int? UserID { get; set; }
        public int? ConsultantID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } 
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
