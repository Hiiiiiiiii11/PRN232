using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects
{
    /// <summary>
    /// Represents individual time slots that can be booked
    /// </summary>
    public class AppointmentSlot
    {
        [Key]
        public int SlotID { get; set; }
        
        public int ConsultantID { get; set; }
        
        public DateTime SlotDateTime { get; set; }
        
        /// <summary>
        /// Duration of the slot in minutes
        /// </summary>
        public int DurationMinutes { get; set; } = 30;
        
        /// <summary>
        /// Available, Booked, Blocked
        /// </summary>
        public string Status { get; set; } = "Available";
        
        public int? AppointmentID { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; } = null!;
        
        [ForeignKey("AppointmentID")]
        public virtual Appointment? Appointment { get; set; }
    }
}