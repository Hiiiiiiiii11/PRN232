using BussinessObjects;
using Repositories.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Courses
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        // Course specific queries - chỉ work với Entity models
        Task<Course?> GetCourseWithDetailsAsync(int courseId);
        Task<BasePaginatedList<Course>> GetCoursesWithFiltersAsync(
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
            int pageSize = 10);
        Task<List<Course>> GetCoursesByCreatorAsync(int creatorId);
        Task<List<Course>> GetCoursesByTargetGroupAsync(string targetGroup);
        Task<List<Course>> GetCoursesByAgeGroupAsync(string ageGroup);
        Task<bool> ExistsCourseByTitleAsync(string title, int? excludeCourseId = null);
        Task<List<Course>> GetPendingApprovalCoursesAsync();
        Task<List<Course>> GetActiveCoursesAsync();
        Task<int> GetTotalRegistrationsCountAsync(int courseId);
        Task<int> GetTotalContentsCountAsync(int courseId);
        Task<bool> HasRegistrationsAsync(int courseId);
    }
}
