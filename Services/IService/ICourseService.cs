using BussinessObjects;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.CourseContent;
using Services.DTOs.Courses;
using Services.DTOs.Dashboard;
using Services.DTOs.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{    public interface ICourseService
    {
        // Course Management
        Task<CourseResponseDto> GetCourseByIdAsync(int courseId);
        Task<BasePaginatedList<CourseListDto>> GetCoursesAsync(CourseFilterDto filter);
        Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto createDto, int createdBy);
        Task<CourseResponseDto> UpdateCourseAsync(UpdateCourseDto updateDto, int updatedBy);
        Task DeleteCourseAsync(int courseId, int deletedBy);
        Task ToggleCourseStatusAsync(int courseId, bool isActive, int updatedBy);
        Task ApproveCourseAsync(int courseId, bool isAccept, int approvedBy);

        // Course Content Management
        Task<BasePaginatedList<CourseContentResponseDto>> GetCourseContentsAsync(int courseId, int pageIndex = 1, int pageSize = 10);
        Task<CourseContentResponseDto> GetCourseContentByIdAsync(int contentId);
        Task<CourseContentResponseDto> CreateCourseContentAsync(CreateCourseContentDto createDto);
        Task<CourseContentResponseDto> UpdateCourseContentAsync(UpdateCourseContentDto updateDto);
        Task DeleteCourseContentAsync(int contentId);
        Task ReorderCourseContentsAsync(int courseId, Dictionary<int, int> contentOrderMapping);

        // Additional content methods
        Task<List<CourseContentResponseDto>> GetActiveContentsAsync(int courseId);
        Task<int> GetNextOrderIndexAsync(int courseId);

        // Course Registration Management
        Task<CourseRegistrationResponseDto> RegisterForCourseAsync(int courseId, int userId);
        Task UnregisterFromCourseAsync(int courseId, int userId);
        Task<CourseRegistrationResponseDto> GetRegistrationAsync(int courseId, int userId);
        Task<BasePaginatedList<RegistrationListDto>> GetCourseRegistrationsAsync(int courseId, RegistrationFilterDto filter);
        Task<BasePaginatedList<RegistrationListDto>> GetUserRegistrationsAsync(int userId, RegistrationFilterDto filter);
        Task<UserLearningDashboardDto> GetUserDashboardAsync(int userId);
        Task<CourseEnrollmentStatsDto> GetCourseEnrollmentStatsAsync(int courseId);
        Task<CourseRegistrationResponseDto> UpdateProgressAsync(UpdateProgressDto updateDto, int userId);

        // Registration validation
        Task<bool> IsUserRegisteredAsync(int courseId, int userId);
        Task<bool> CanUserRegisterAsync(int courseId, int userId);
    }
}
