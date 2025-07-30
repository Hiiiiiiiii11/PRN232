using System;

namespace Services.DTOs.Consultant
{
    public class ConsultantAvailabilityDto
    {
        public int AvailabilityID { get; set; }
        public int ConsultantID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
    
    public class CreateConsultantAvailabilityDto
    {
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = "Available";
        public string? Notes { get; set; }
    }
    
    public class UpdateConsultantAvailabilityDto
    {
        public int AvailabilityID { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}