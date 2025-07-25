using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.ConsultantUser;
using Services.DTOs.Courses;

namespace Services.IService
{
    public interface IConsultantUserService
    {
        // Get consultants
        Task<BasePaginatedList<ConsultantDto>> GetAllConsultantsAsync(ConsultantFilterDto filter);
        Task<BasePaginatedList<ConsultantDto>> GetAllActiveConsultantsAsync(
            ConsultantFilterDto filter
        );
        Task<ConsultantDto> GetConsultantByIdAsync(int consultantId);
        Task<ConsultantDto> GetConsultantByUserIdAsync(int userId);

        // Get consultant's courses
        Task<BasePaginatedList<CourseDto>> GetConsultantCoursesAsync(
            int consultantId,
            PagingRequest pagingRequest
        );

        // Search consultants
        Task<BasePaginatedList<ConsultantDto>> SearchConsultantsAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        );
        Task<BasePaginatedList<ConsultantDto>> GetConsultantsBySpecialtyAsync(
            string specialty,
            PagingRequest pagingRequest
        );

        // CRUD operations
        Task UpdateConsultantStatusAsync(int consultantId, string status, int updatedById);

        // Statistics
        Task<ConsultantStatsDto> GetConsultantStatsAsync();
    }
}
