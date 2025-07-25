using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Courses;
using Repositories.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Courses
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents.OrderBy(cc => cc.OrderIndex))
                .Include(c => c.Registrations)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
        }

        public async Task<BasePaginatedList<Course>> GetCoursesWithFiltersAsync(
            string? searchKeyword = null,
            string? targetGroup = null,
            string? ageGroup = null,
            //string? skills = null, // NEW: Skills filter
            bool? isActive = null,
            bool? isAccept = null,
            int? createdBy = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            var query = Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents)
                .Include(c => c.Registrations)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(keyword) ||
                                       c.Description.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrEmpty(targetGroup))
            {
                query = query.Where(c => c.TargetGroup == targetGroup);
            }

            if (!string.IsNullOrEmpty(ageGroup))
            {
                query = query.Where(c => c.AgeGroup == ageGroup);
            }
           

            if (isActive.HasValue)
            {
                query = query.Where(c => c.isActive == isActive.Value);
            }

            if (isAccept.HasValue)
            {
                query = query.Where(c => c.isAccept == isAccept.Value);
            }

            if (createdBy.HasValue)
            {
                query = query.Where(c => c.CreatedBy == createdBy.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= toDate.Value);
            }

            // Order by CreatedAt descending
            query = query.OrderByDescending(c => c.CreatedAt);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<Course>> GetCoursesByCreatorAsync(int creatorId)
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents)
                .Include(c => c.Registrations)
                .Where(c => c.CreatedBy == creatorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByTargetGroupAsync(string targetGroup)
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents.Where(cc => cc.isActive).OrderBy(cc => cc.OrderIndex))
                .Where(c => c.TargetGroup == targetGroup && c.isActive && c.isAccept)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByAgeGroupAsync(string ageGroup)
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents.Where(cc => cc.isActive).OrderBy(cc => cc.OrderIndex))
                .Where(c => c.AgeGroup == ageGroup && c.isActive && c.isAccept)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsCourseByTitleAsync(string title, int? excludeCourseId = null)
        {
            var query = Entities.Where(c => c.Title.ToLower() == title.ToLower());

            if (excludeCourseId.HasValue)
            {
                query = query.Where(c => c.CourseID != excludeCourseId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Course>> GetPendingApprovalCoursesAsync()
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents)
                .Where(c => !c.isAccept)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetActiveCoursesAsync()
        {
            return await Entities
                .Include(c => c.Creator)
                .Include(c => c.Contents.Where(cc => cc.isActive).OrderBy(cc => cc.OrderIndex))
                .Where(c => c.isActive && c.isAccept)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalRegistrationsCountAsync(int courseId)
        {
            return await Entities
                .Where(c => c.CourseID == courseId)
                .SelectMany(c => c.Registrations)
                .CountAsync();
        }

        public async Task<int> GetTotalContentsCountAsync(int courseId)
        {
            return await Entities
                .Where(c => c.CourseID == courseId)
                .SelectMany(c => c.Contents)
                .CountAsync();
        }

        public async Task<bool> HasRegistrationsAsync(int courseId)
        {
            return await Entities
                .Where(c => c.CourseID == courseId)
                .SelectMany(c => c.Registrations)
                .AnyAsync();
        }
    }
}
