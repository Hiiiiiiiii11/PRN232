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
    public class CourseContentRepository : GenericRepository<CourseContent>, ICourseContentRepository
    {
        public CourseContentRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<CourseContent?> GetContentWithCourseAsync(int contentId)
        {
            return await Entities
                .Include(cc => cc.Course)
                .ThenInclude(c => c.Creator)
                .FirstOrDefaultAsync(cc => cc.ContentID == contentId);
        }

        public async Task<BasePaginatedList<CourseContent>> GetContentsByCourseAsync(int courseId, int pageIndex, int pageSize)
        {
            var query = Entities
                .Include(cc => cc.Course)
                .Where(cc => cc.CourseID == courseId)
                .OrderBy(cc => cc.OrderIndex);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<CourseContent>> GetContentsByCourseOrderedAsync(int courseId)
        {
            return await Entities
                .Include(cc => cc.Course)
                .Where(cc => cc.CourseID == courseId)
                .OrderBy(cc => cc.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<CourseContent>> GetActiveContentsByCourseAsync(int courseId)
        {
            return await Entities
                .Include(cc => cc.Course)
                .Where(cc => cc.CourseID == courseId && cc.isActive)
                .OrderBy(cc => cc.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> ExistsContentWithOrderAsync(int courseId, int orderIndex, int? excludeContentId = null)
        {
            var query = Entities.Where(cc => cc.CourseID == courseId && cc.OrderIndex == orderIndex);

            if (excludeContentId.HasValue)
            {
                query = query.Where(cc => cc.ContentID != excludeContentId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetMaxOrderIndexAsync(int courseId)
        {
            var maxOrder = await Entities
                .Where(cc => cc.CourseID == courseId)
                .MaxAsync(cc => (int?)cc.OrderIndex);

            return maxOrder ?? 0;
        }

        public async Task<int> GetNextOrderIndexAsync(int courseId)
        {
            var maxOrder = await GetMaxOrderIndexAsync(courseId);
            return maxOrder + 1;
        }

        public async Task<List<CourseContent>> GetContentsByTypeAsync(int courseId, string contentType)
        {
            return await Entities
                .Include(cc => cc.Course)
                .Where(cc => cc.CourseID == courseId && cc.ContentType == contentType)
                .OrderBy(cc => cc.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> HasProgressRecordsAsync(int contentId)
        {
            return await Entities
                .Where(cc => cc.ContentID == contentId)
                .SelectMany(cc => cc.ProgressByUsers)
                .AnyAsync();
        }

        public async Task<CourseContent?> GetNextContentAsync(int courseId, int currentOrderIndex)
        {
            return await Entities
                .Where(cc => cc.CourseID == courseId &&
                           cc.OrderIndex > currentOrderIndex &&
                           cc.isActive)
                .OrderBy(cc => cc.OrderIndex)
                .FirstOrDefaultAsync();
        }

        public async Task<CourseContent?> GetPreviousContentAsync(int courseId, int currentOrderIndex)
        {
            return await Entities
                .Where(cc => cc.CourseID == courseId &&
                           cc.OrderIndex < currentOrderIndex &&
                           cc.isActive)
                .OrderByDescending(cc => cc.OrderIndex)
                .FirstOrDefaultAsync();
        }
    }

}



