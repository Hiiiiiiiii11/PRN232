using BussinessObjects;

namespace Repositories.IRepository.Consultants
{
    public interface IAppointmentSlotRepository : IGenericRepository<AppointmentSlot>
    {
        Task<List<AppointmentSlot>> GetSlotsByConsultantIdAsync(int consultantId);
        Task<List<AppointmentSlot>> GetAvailableSlotsByConsultantAndDateAsync(int consultantId, DateTime date);
        Task<List<AppointmentSlot>> GetSlotsByDateRangeAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<AppointmentSlot?> GetSlotByDateTimeAsync(int consultantId, DateTime slotDateTime);
        Task<bool> IsSlotAvailableAsync(int consultantId, DateTime slotDateTime);
        Task GenerateSlotsForDateAsync(int consultantId, DateTime date);
        Task<List<AppointmentSlot>> GetBookedSlotsByConsultantAsync(int consultantId, DateTime startDate, DateTime endDate);
    }
}