using BussinessObjects;

namespace Repositories.IRepository.Consultants
{
    public interface IConsultantAvailabilityRepository : IGenericRepository<ConsultantAvailability>
    {
        Task<List<ConsultantAvailability>> GetAvailabilitiesByConsultantIdAsync(int consultantId);
        Task<List<ConsultantAvailability>> GetAvailabilitiesByDateRangeAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<ConsultantAvailability?> GetAvailabilityByDateAsync(int consultantId, DateTime date);
        Task<bool> IsConsultantAvailableAsync(int consultantId, DateTime date);
    }
}