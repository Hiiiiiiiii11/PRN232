using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Courses
{
    public interface ICourseRegistrationRepository : IGenericRepository<CourseRegistration>
    {
        // Registration specific queries
        Task<bool> IsUserRegisteredAsync(int userId, int courseId);
        Task<CourseRegistration?> GetRegistrationAsync(int userId, int courseId);
        Task<List<CourseRegistration>> GetUserRegistrationsAsync(int userId);
        Task<List<CourseRegistration>> GetCourseRegistrationsAsync(int courseId);
        Task<int> GetCourseRegistrationCountAsync(int courseId);
        Task<List<CourseRegistration>> GetRegistrationsWithFiltersAsync(int? userId = null, int? courseId = null, bool? completed = null);
        Task<CourseRegistration?> GetRegistrationWithDetailsAsync(int registrationId);
    }
}
