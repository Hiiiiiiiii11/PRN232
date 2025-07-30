using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Consultants;

namespace Repositories.Repository.Consultants
{
    public class ConsultantScheduleRepository : GenericRepository<ConsultantSchedule>, IConsultantScheduleRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public ConsultantScheduleRepository(DrugUsePreventionDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ConsultantSchedule>> GetSchedulesByConsultantIdAsync(int consultantId)
        {
            return await _context.ConsultantSchedules
                .Where(cs => cs.ConsultantID == consultantId)
                .OrderBy(cs => cs.DayOfWeek)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();
        }

        public async Task<List<ConsultantSchedule>> GetActiveSchedulesByConsultantIdAsync(int consultantId)
        {
            return await _context.ConsultantSchedules
                .Where(cs => cs.ConsultantID == consultantId && cs.IsActive)
                .OrderBy(cs => cs.DayOfWeek)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();
        }

        public async Task<ConsultantSchedule?> GetScheduleByConsultantAndDayAsync(int consultantId, int dayOfWeek)
        {
            return await _context.ConsultantSchedules
                .FirstOrDefaultAsync(cs => cs.ConsultantID == consultantId 
                                        && cs.DayOfWeek == dayOfWeek 
                                        && cs.IsActive);
        }

        public async Task<bool> HasScheduleConflictAsync(int consultantId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null)
        {
            var query = _context.ConsultantSchedules
                .Where(cs => cs.ConsultantID == consultantId 
                          && cs.DayOfWeek == dayOfWeek 
                          && cs.IsActive
                          && ((cs.StartTime <= startTime && cs.EndTime > startTime) ||
                              (cs.StartTime < endTime && cs.EndTime >= endTime) ||
                              (cs.StartTime >= startTime && cs.EndTime <= endTime)));

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(cs => cs.ScheduleID != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }
    }
}