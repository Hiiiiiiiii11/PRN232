using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects
{
    /// <summary>
    /// Represents specific date availability exceptions for a consultant
    /// (overrides the regular schedule for specific dates)
    /// </summary>
    public class ConsultantAvailability
    {
        [Key]
        public int AvailabilityID { get; set; }
        
        public int ConsultantID { get; set; }
        
        public DateTime Date { get; set; }
        
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
        /// <summary>
        /// Available, Unavailable, Custom
        /// </summary>
        public string Status { get; set; } = "Available";
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; } = null!;
    }
}