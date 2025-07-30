using System;

namespace Services.DTOs.Consultant
{
    public class AppointmentSlotDto
    {
        public int SlotID { get; set; }
        public int ConsultantID { get; set; }
        public DateTime SlotDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public int? AppointmentID { get; set; }
        public bool IsAvailable => Status == "Available";
        public string FormattedTime => SlotDateTime.ToString("HH:mm");
        public string FormattedDate => SlotDateTime.ToString("dd/MM/yyyy");
    }
    
    public class ConsultantCalendarDto
    {
        public int ConsultantID { get; set; }
        public DateTime Date { get; set; }
        public List<AppointmentSlotDto> AvailableSlots { get; set; } = new List<AppointmentSlotDto>();
        public List<AppointmentSlotDto> BookedSlots { get; set; } = new List<AppointmentSlotDto>();
        public bool HasAvailability { get; set; }
    }
    
    public class WeeklyCalendarDto
    {
        public int ConsultantID { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<ConsultantCalendarDto> DailyCalendars { get; set; } = new List<ConsultantCalendarDto>();
        public List<ConsultantScheduleDto> WeeklySchedule { get; set; } = new List<ConsultantScheduleDto>();
    }
}