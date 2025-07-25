using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.Paging;

namespace Repositories.IRepository.Categories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    { // Category specific queries - chỉ work với Entity models
        Task<Category?> GetCategoryWithDetailsAsync(int categoryId);

        Task<BasePaginatedList<Category>> GetCategoriesWithFiltersAsync(
            string? searchKeyword = null,
            int pageIndex = 1,
            int pageSize = 10
        );

        Task<List<Category>> GetActiveCategoriesAsync();

        Task<List<Category>> GetInactiveCategoriesAsync();

        Task<List<Category>> GetCategoriesWithNewsCountAsync();

        Task<Category?> GetMostUsedCategoryAsync();

        Task<List<Category>> GetTopCategoriesByUsageAsync(int count = 5);

        Task<int> GetNewsCountByCategoryAsync(int categoryId);
        Task<bool> ExistsCategoryByNameAsync(string categoryName, int? excludeCategoryId = null);
    }
}
