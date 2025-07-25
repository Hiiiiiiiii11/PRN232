using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.Paging;

namespace Repositories.IRepository.NewsArticles
{
    public interface INewsArticleRepository : IGenericRepository<NewsArticle>
    {
        // NewsArticle specific queries - chỉ work với Entity models
        Task<NewsArticle?> GetNewsArticleWithDetailsAsync(int newsArticleId);

        Task<BasePaginatedList<NewsArticle>> GetNewsArticlesWithFiltersAsync(
            string? searchKeyword = null,
            int? categoryId = null,
            string? newsSource = null,
            string? newsStatus = null,
            int? createdBy = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 10
        );

        Task<List<NewsArticle>> GetNewsArticlesByCategoryAsync(int categoryId);

        Task<List<NewsArticle>> GetNewsArticlesBySourceAsync(string newsSource);

        Task<List<NewsArticle>> GetNewsArticlesByCreatorAsync(int createdBy);

        Task<List<NewsArticle>> GetNewsArticlesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate
        );

        Task<bool> ExistsNewsByTitleAsync(string title, int? excludeNewsId = null);

        Task<List<NewsArticle>> GetActiveNewsArticlesAsync();

        Task<List<NewsArticle>> GetInactiveNewsArticlesAsync();

        Task<List<NewsArticle>> GetRecentNewsArticlesAsync(int count = 10);

        Task<List<NewsArticle>> GetTrendingNewsArticlesAsync(int count = 10);

        Task<int> GetTotalActiveNewsCountAsync();

        Task<int> GetTotalInactiveNewsCountAsync();

        Task<int> GetTodayNewsCountAsync();

        Task<int> GetThisWeekNewsCountAsync();

        Task<int> GetThisMonthNewsCountAsync();

        Task<bool> IsNewsReferencedAsync(int newsArticleId);
    }
}
