using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.IRepository.Categories;
using Repositories.Paging;

namespace Repositories.Repository.Categories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<Category?> GetCategoryWithDetailsAsync(int categoryId)
        {
            return await Entities
                .Include(c => c.NewsAticles.Where(n => n.NewsStatus == "Active"))
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);
        }

        public async Task<BasePaginatedList<Category>> GetCategoriesWithFiltersAsync(
            string? searchKeyword = null,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var query = Entities.Include(c => c.NewsAticles).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.Trim().ToLower();
                query = query.Where(c =>
                    c.CategoryName.ToLower().Contains(keyword)
                    || (
                        c.CategoryDescription != null
                        && c.CategoryDescription.ToLower().Contains(keyword)
                    )
                );
            }

            // Order by CategoryName
            query = query.OrderBy(c => c.CategoryName);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await Entities
                .Include(c => c.NewsAticles.Where(n => n.NewsStatus == "Active"))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<List<Category>> GetInactiveCategoriesAsync()
        {
            return await Entities
                .Include(c => c.NewsAticles)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesWithNewsCountAsync()
        {
            return await Entities
                .Include(c => c.NewsAticles.Where(n => n.NewsStatus == "Active"))
                .OrderByDescending(c => c.NewsAticles.Count)
                .ToListAsync();
        }

        public async Task<bool> ExistsCategoryByNameAsync(
            string categoryName,
            int? excludeCategoryId = null
        )
        {
            var query = Entities.Where(c => c.CategoryName.ToLower() == categoryName.ToLower());

            if (excludeCategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryID != excludeCategoryId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsCategoryInUseAsync(int categoryId)
        {
            return await Entities
                .Where(c => c.CategoryID == categoryId)
                .SelectMany(c => c.NewsAticles)
                .Where(n => n.NewsStatus == "Active")
                .AnyAsync();
        }

        public async Task<Category?> GetMostUsedCategoryAsync()
        {
            return await Entities
                .Include(c => c.NewsAticles.Where(n => n.NewsStatus == "Active"))
                .OrderByDescending(c => c.NewsAticles.Count)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetTopCategoriesByUsageAsync(int count = 5)
        {
            return await Entities
                .Include(c => c.NewsAticles.Where(n => n.NewsStatus == "Active"))
                .OrderByDescending(c => c.NewsAticles.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetNewsCountByCategoryAsync(int categoryId)
        {
            return await Entities
                .Where(c => c.CategoryID == categoryId)
                .SelectMany(c => c.NewsAticles)
                .Where(n => n.NewsStatus == "Active")
                .CountAsync();
        }
    }
}
