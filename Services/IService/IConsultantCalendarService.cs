using Services.DTOs.Consultant;

namespace Services.IService
{
    public interface IConsultantCalendarService
    {
        // Schedule Management
        Task<List<ConsultantScheduleDto>> GetConsultantScheduleAsync(int consultantId);
        Task<ConsultantScheduleDto> CreateScheduleAsync(int consultantId, CreateConsultantScheduleDto createDto);
        Task<ConsultantScheduleDto> UpdateScheduleAsync(int consultantId, UpdateConsultantScheduleDto updateDto);
        Task<bool> DeleteScheduleAsync(int consultantId, int scheduleId);

        // Availability Management
        Task<List<ConsultantAvailabilityDto>> GetConsultantAvailabilityAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<ConsultantAvailabilityDto> SetAvailabilityAsync(int consultantId, CreateConsultantAvailabilityDto createDto);
        Task<ConsultantAvailabilityDto> UpdateAvailabilityAsync(int consultantId, UpdateConsultantAvailabilityDto updateDto);
        Task<bool> DeleteAvailabilityAsync(int consultantId, int availabilityId);

        // Calendar Views
        Task<WeeklyCalendarDto> GetWeeklyCalendarAsync(int consultantId, DateTime weekStartDate);
        Task<ConsultantCalendarDto> GetDailyCalendarAsync(int consultantId, DateTime date);
        Task<List<AppointmentSlotDto>> GetAvailableSlotsAsync(int consultantId, DateTime date);

        // Slot Management
        Task GenerateSlotsForPeriodAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<bool> BookSlotAsync(int consultantId, DateTime slotDateTime, int appointmentId);
        Task<bool> ReleaseSlotAsync(int consultantId, DateTime slotDateTime);

        // Utility Methods
        Task<bool> IsSlotAvailableAsync(int consultantId, DateTime slotDateTime);
        Task<List<DateTime>> GetNextAvailableSlotsAsync(int consultantId, int count = 10);
    }
}