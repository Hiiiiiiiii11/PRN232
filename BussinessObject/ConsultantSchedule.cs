using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects
{
    /// <summary>
    /// Represents a consultant's recurring weekly schedule
    /// </summary>
    public class ConsultantSchedule
    {
        [Key]
        public int ScheduleID { get; set; }
        
        public int ConsultantID { get; set; }
        
        /// <summary>
        /// Day of week (0 = Sunday, 1 = Monday, etc.)
        /// </summary>
        public int DayOfWeek { get; set; }
        
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Duration of each appointment slot in minutes
        /// </summary>
        public int SlotDurationMinutes { get; set; } = 30;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; } = null!;
    }
}