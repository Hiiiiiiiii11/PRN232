using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Appointment
{
    public class AppointmentRequest
    {
        public int? ConsultantID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Notes { get; set; }
    }
}
