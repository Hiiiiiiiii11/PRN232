using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Courses
{
    public class CourseRegistrationRepository : GenericRepository<CourseRegistration>, ICourseRegistrationRepository
    {
        public CourseRegistrationRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<bool> IsUserRegisteredAsync(int userId, int courseId)
        {
            return await Entities
                .AnyAsync(r => r.UserID == userId && r.CourseID == courseId);
        }

        public async Task<CourseRegistration?> GetRegistrationAsync(int userId, int courseId)
        {
            return await Entities
                .Include(r => r.User)
                .Include(r => r.Course)
                .ThenInclude(c => c.Creator)
                .FirstOrDefaultAsync(r => r.UserID == userId && r.CourseID == courseId);
        }

        public async Task<List<CourseRegistration>> GetUserRegistrationsAsync(int userId)
        {
            return await Entities
                .Include(r => r.Course)
                .ThenInclude(c => c.Creator)
                .Include(r => r.Course.Contents.Where(cc => cc.isActive).OrderBy(cc => cc.OrderIndex))
                .Where(r => r.UserID == userId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<List<CourseRegistration>> GetCourseRegistrationsAsync(int courseId)
        {
            return await Entities
                .Include(r => r.User)
                .Where(r => r.CourseID == courseId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<int> GetCourseRegistrationCountAsync(int courseId)
        {
            return await Entities
                .Where(r => r.CourseID == courseId)
                .CountAsync();
        }

        public async Task<List<CourseRegistration>> GetRegistrationsWithFiltersAsync(int? userId = null, int? courseId = null, bool? completed = null)
        {
            var query = Entities
                .Include(r => r.User)
                .Include(r => r.Course)
                .ThenInclude(c => c.Creator)
                .Include(r => r.ContentProgress)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(r => r.UserID == userId.Value);
            }

            if (courseId.HasValue)
            {
                query = query.Where(r => r.CourseID == courseId.Value);
            }

            if (completed.HasValue)
            {
                query = query.Where(r => r.Completed == completed.Value);
            }

            return await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<CourseRegistration?> GetRegistrationWithDetailsAsync(int registrationId)
        {
            return await Entities
                .Include(r => r.User)
                .Include(r => r.Course)
                .ThenInclude(c => c.Creator)
                .Include(r => r.Course.Contents.Where(cc => cc.isActive).OrderBy(cc => cc.OrderIndex))
                .Include(r => r.ContentProgress)
                .ThenInclude(cp => cp.Content)
                .FirstOrDefaultAsync(r => r.RegistrationID == registrationId);
        }
    }
}


