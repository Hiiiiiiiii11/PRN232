using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Consultants;

namespace Repositories.Repository.Consultants
{
    public class ConsultantAvailabilityRepository : GenericRepository<ConsultantAvailability>, IConsultantAvailabilityRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public ConsultantAvailabilityRepository(DrugUsePreventionDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ConsultantAvailability>> GetAvailabilitiesByConsultantIdAsync(int consultantId)
        {
            return await _context.ConsultantAvailabilities
                .Where(ca => ca.ConsultantID == consultantId)
                .OrderBy(ca => ca.Date)
                .ToListAsync();
        }

        public async Task<List<ConsultantAvailability>> GetAvailabilitiesByDateRangeAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            return await _context.ConsultantAvailabilities
                .Where(ca => ca.ConsultantID == consultantId 
                          && ca.Date >= startDate.Date 
                          && ca.Date <= endDate.Date)
                .OrderBy(ca => ca.Date)
                .ToListAsync();
        }

        public async Task<ConsultantAvailability?> GetAvailabilityByDateAsync(int consultantId, DateTime date)
        {
            return await _context.ConsultantAvailabilities
                .FirstOrDefaultAsync(ca => ca.ConsultantID == consultantId 
                                        && ca.Date.Date == date.Date);
        }

        public async Task<bool> IsConsultantAvailableAsync(int consultantId, DateTime date)
        {
            var availability = await GetAvailabilityByDateAsync(consultantId, date);
            return availability?.Status == "Available" || availability == null;
        }
    }
}