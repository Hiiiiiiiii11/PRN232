using BussinessObjects;

namespace Repositories.IRepository.Consultants
{
    public interface IConsultantScheduleRepository : IGenericRepository<ConsultantSchedule>
    {
        Task<List<ConsultantSchedule>> GetSchedulesByConsultantIdAsync(int consultantId);
        Task<List<ConsultantSchedule>> GetActiveSchedulesByConsultantIdAsync(int consultantId);
        Task<ConsultantSchedule?> GetScheduleByConsultantAndDayAsync(int consultantId, int dayOfWeek);
        Task<bool> HasScheduleConflictAsync(int consultantId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null);
    }
}