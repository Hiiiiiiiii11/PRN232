using BussinessObjects;
using Repositories.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Courses
{
    public interface ICourseContentRepository : IGenericRepository<CourseContent>
    {
        // CourseContent specific queries
        Task<CourseContent?> GetContentWithCourseAsync(int contentId);
        Task<BasePaginatedList<CourseContent>> GetContentsByCourseAsync(int courseId, int pageIndex, int pageSize);
        Task<List<CourseContent>> GetActiveContentsByCourseAsync(int courseId);
        Task<List<CourseContent>> GetContentsByCourseOrderedAsync(int courseId);
        Task<bool> ExistsContentWithOrderAsync(int courseId, int orderIndex, int? excludeContentId = null);
        Task<int> GetMaxOrderIndexAsync(int courseId);
        Task<int> GetNextOrderIndexAsync(int courseId);
        Task<List<CourseContent>> GetContentsByTypeAsync(int courseId, string contentType);
        Task<bool> HasProgressRecordsAsync(int contentId);
        Task<CourseContent?> GetNextContentAsync(int courseId, int currentOrderIndex);
        Task<CourseContent?> GetPreviousContentAsync(int courseId, int currentOrderIndex);
    }
}
