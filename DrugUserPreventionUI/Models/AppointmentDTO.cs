namespace DrugUserPreventionUI.Models
{
    public class AppointmentDTO
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
