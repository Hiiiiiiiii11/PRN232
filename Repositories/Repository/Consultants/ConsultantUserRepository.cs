using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Consultants;
using Repositories.Paging;

namespace Repositories.Repository.Consultants
{
    public class ConsultantUserRepository : GenericRepository<Consultant>, IConsultantUserRepository
    {
        public ConsultantUserRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<Consultant?> GetConsultantWithDetailsAsync(int consultantId)
        {
            return await Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .FirstOrDefaultAsync(c => c.ConsultantID == consultantId);
        }

        public async Task<Consultant?> GetConsultantByUserIdAsync(int userId)
        {
            return await Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .FirstOrDefaultAsync(c => c.User.UserID == userId);
        }

        public async Task<BasePaginatedList<Consultant>> GetConsultantsWithFiltersAsync(
            string? searchKeyword = null,
            string? specialty = null,
            string? status = null,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var query = Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.Trim().ToLower();
                query = query.Where(c =>
                    c.User.FullName.ToLower().Contains(keyword)
                    || c.User.Username.ToLower().Contains(keyword)
                    || c.User.Email.ToLower().Contains(keyword)
                    || c.Qualifications.ToLower().Contains(keyword)
                    || c.Specialty.ToLower().Contains(keyword)
                );
            }

            if (!string.IsNullOrEmpty(specialty))
            {
                query = query.Where(c => c.Specialty.ToLower().Contains(specialty.ToLower()));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.User.Status == status);
            }

            // Order by User creation date descending
            query = query.OrderByDescending(c => c.User.CreatedAt);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<Course>> GetConsultantCoursesAsync(int consultantId)
        {
            var consultant = await Entities
                .Include(c => c.User)
                .ThenInclude(u => u.CreatedCourses)
                .FirstOrDefaultAsync(c => c.ConsultantID == consultantId);

            return consultant?.User?.CreatedCourses?.ToList() ?? new List<Course>();
        }

        // Fix for CS0308: Update the GetPagging method call to use the correct signature without type arguments.
        // Assuming the GenericRepository class has a non-generic GetPagging method that works with IQueryable<T>.

        public async Task<BasePaginatedList<Course>> GetConsultantCoursesPagedAsync(
            int consultantId,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var consultant = await Entities
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ConsultantID == consultantId);

            if (consultant?.User == null)
            {
                return new BasePaginatedList<Course>(new List<Course>(), 0, pageIndex, pageSize);
            }

            var coursesQuery = _context
                .Set<Course>()
                .Where(course => course.CreatedBy == consultant.User.UserID)
                .OrderByDescending(course => course.CreatedAt);

            // Manual pagination for Course entities
            var totalItems = await coursesQuery.CountAsync();
            var items = await coursesQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Course>(items, totalItems, pageIndex, pageSize);
        }

        public async Task<List<Consultant>> GetConsultantsBySpecialtyAsync(string specialty)
        {
            return await Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .Where(c =>
                    c.Specialty.ToLower().Contains(specialty.ToLower()) && c.User.Status == "Active"
                )
                .OrderByDescending(c => c.User.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsConsultantByUserIdAsync(
            int userId,
            int? excludeConsultantId = null
        )
        {
            var query = Entities.Where(c => c.User.UserID == userId);

            if (excludeConsultantId.HasValue)
            {
                query = query.Where(c => c.ConsultantID != excludeConsultantId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Consultant>> GetActiveConsultantsAsync()
        {
            return await Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .Where(c => c.User.Status == "Active")
                .OrderByDescending(c => c.User.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Consultant>> GetInactiveConsultantsAsync()
        {
            return await Entities
                .Include(c => c.User)
                .Where(c => c.User.Status == "Inactive")
                .OrderByDescending(c => c.User.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalActiveConsultantsCountAsync()
        {
            return await Entities.CountAsync(c => c.User.Status == "Active");
        }

        public async Task<int> GetTotalInactiveConsultantsCountAsync()
        {
            return await Entities.CountAsync(c => c.User.Status == "Inactive");
        }

        public async Task<int> GetTotalCoursesCountAsync()
        {
            return await _context
                .Set<Course>()
                .Where(course => Entities.Any(c => c.User.UserID == course.CreatedBy))
                .CountAsync();
        }

        public async Task<List<Consultant>> GetRecentConsultantsAsync(int count = 5)
        {
            return await Entities
                .Include(c => c.User)
                .Include(c => c.User.CreatedCourses)
                .Where(c => c.User.Status == "Active")
                .OrderByDescending(c => c.User.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsConsultantReferencedAsync(int consultantId)
        {
            // Check if consultant is referenced by appointments
            var hasAppointments = await _context
                .Set<Appointment>()
                .AnyAsync(a => a.ConsultantID == consultantId);

            return hasAppointments;
        }
    }
}
