using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.Paging;

namespace Repositories.IRepository.Consultants
{
    public interface IConsultantUserRepository : IGenericRepository<Consultant>
    {
        // Get consultant with details
        Task<Consultant?> GetConsultantWithDetailsAsync(int consultantId);
        Task<Consultant?> GetConsultantByUserIdAsync(int userId);

        // Get consultants with filters and pagination
        Task<BasePaginatedList<Consultant>> GetConsultantsWithFiltersAsync(
            string? searchKeyword = null,
            string? specialty = null,
            string? status = null,
            int pageIndex = 1,
            int pageSize = 10
        );

        // Get consultant's courses
        Task<List<Course>> GetConsultantCoursesAsync(int consultantId);
        Task<BasePaginatedList<Course>> GetConsultantCoursesPagedAsync(
            int consultantId,
            int pageIndex = 1,
            int pageSize = 10
        );

        // Get consultants by specialty
        Task<List<Consultant>> GetConsultantsBySpecialtyAsync(string specialty);

        // Validation methods
        Task<bool> ExistsConsultantByUserIdAsync(int userId, int? excludeConsultantId = null);

        // Get active/inactive consultants
        Task<List<Consultant>> GetActiveConsultantsAsync();
        Task<List<Consultant>> GetInactiveConsultantsAsync();

        // Statistics methods
        Task<int> GetTotalActiveConsultantsCountAsync();
        Task<int> GetTotalInactiveConsultantsCountAsync();
        Task<int> GetTotalCoursesCountAsync();
        Task<List<Consultant>> GetRecentConsultantsAsync(int count = 5);

        // Check if consultant is referenced by other entities
        Task<bool> IsConsultantReferencedAsync(int consultantId);
    }
}
