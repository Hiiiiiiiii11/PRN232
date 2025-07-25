using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.IRepository.NewsArticles;
using Repositories.Paging;

namespace Repositories.Repository.NewsArticles
{
    public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        public NewsArticleRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<NewsArticle?> GetNewsArticleWithDetailsAsync(int newsArticleId)
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.UpdatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(n => n.NewsArticleID == newsArticleId);
        }

        public async Task<BasePaginatedList<NewsArticle>> GetNewsArticlesWithFiltersAsync(
            string? searchKeyword = null,
            int? categoryId = null,
            string? newsSource = null,
            string? newsStatus = null,
            int? createdBy = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var query = Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.UpdatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.Trim().ToLower();
                query = query.Where(n =>
                    n.NewsAticleName.ToLower().Contains(keyword)
                    || n.Headline.ToLower().Contains(keyword)
                    || n.NewsContent.ToLower().Contains(keyword)
                );
            }

            if (categoryId.HasValue)
            {
                query = query.Where(n => n.CategoryID == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(newsSource))
            {
                query = query.Where(n => n.NewsSource.ToLower().Contains(newsSource.ToLower()));
            }

            if (newsStatus != null)
            {
                query = query.Where(n => n.NewsStatus == newsStatus);
            }

            if (createdBy.HasValue)
            {
                query = query.Where(n => n.CreatedByID == createdBy.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate <= toDate.Value);
            }

            // Order by CreatedDate descending
            query = query.OrderByDescending(n => n.CreatedDate);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByCategoryAsync(int categoryId)
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .Where(n => n.CategoryID == categoryId && n.NewsStatus == "Active")
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetNewsArticlesBySourceAsync(string newsSource)
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .Where(n =>
                    n.NewsSource.ToLower().Contains(newsSource.ToLower())
                    && n.NewsStatus == "Active"
                )
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByCreatorAsync(int createdBy)
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .Where(n => n.CreatedByID == createdBy)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Where(n =>
                    n.CreatedDate >= startDate
                    && n.CreatedDate <= endDate
                    && n.NewsStatus == "Active"
                )
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ExistsNewsByTitleAsync(string title, int? excludeNewsId = null)
        {
            var query = Entities.Where(n => n.NewsAticleName.ToLower() == title.ToLower());

            if (excludeNewsId.HasValue)
            {
                query = query.Where(n => n.NewsArticleID != excludeNewsId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<NewsArticle>> GetActiveNewsArticlesAsync()
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
                .Where(n => n.NewsStatus == "Active")
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetInactiveNewsArticlesAsync()
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Where(n => n.NewsStatus == "Inactive")
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetRecentNewsArticlesAsync(int count = 10)
        {
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Where(n => n.NewsStatus == "Active")
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<NewsArticle>> GetTrendingNewsArticlesAsync(int count = 10)
        {
            // Trending logic: Recent articles with high engagement (for now, just recent)
            return await Entities
                .Include(n => n.CreatedBy)
                .Include(n => n.Category)
                .Where(n => n.NewsStatus == "Active" && n.CreatedDate >= DateTime.Now.AddDays(-7))
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTotalActiveNewsCountAsync()
        {
            return await Entities.CountAsync(n => n.NewsStatus == "Actie");
        }

        public async Task<int> GetTotalInactiveNewsCountAsync()
        {
            return await Entities.CountAsync(n => n.NewsStatus == "Inactive");
        }

        public async Task<int> GetTodayNewsCountAsync()
        {
            var today = DateTime.Today;
            return await Entities.CountAsync(n =>
                n.CreatedDate.Date == today && n.NewsStatus == "Active"
            );
        }

        public async Task<int> GetThisWeekNewsCountAsync()
        {
            var weekStart = DateTime.Today.AddDays(-7);
            return await Entities.CountAsync(n =>
                n.CreatedDate >= weekStart && n.NewsStatus == "Active"
            );
        }

        public async Task<int> GetThisMonthNewsCountAsync()
        {
            var monthStart = DateTime.Today.AddMonths(-1);
            return await Entities.CountAsync(n =>
                n.CreatedDate >= monthStart && n.NewsStatus == "Active"
            );
        }

        public async Task<bool> IsNewsReferencedAsync(int newsArticleId)
        {
            // Check if news is referenced by other entities (if any)
            // For now, return false as we don't have references
            return false;
        }
    }
}
