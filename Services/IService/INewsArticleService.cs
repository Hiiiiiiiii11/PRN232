using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.NewArticle;

namespace Services.IService
{
    public interface INewsArticleService
    {
        Task<BasePaginatedList<NewsArticleDto>> GetAllNewsArticlesAsync(
            NewsArticleFilterDto filter
        );
        Task<BasePaginatedList<NewsArticleDto>> GetAllActiveNewsArticlesAsync(
            NewsArticleFilterDto filter
        );
        Task<NewsArticleDto> GetNewsArticleByIdAsync(int newsArticleId);
        Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesByCategoryAsync(
            int categoryId,
            PagingRequest pagingRequest
        );
        Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesBySourceAsync(
            string newsSource,
            PagingRequest pagingRequest
        );
        Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            PagingRequest pagingRequest
        );
        Task<NewsArticleDto> AddNewsArticleAsync(CreateNewsArticleDto createDto, int createdById);
        Task<NewsArticleDto> UpdateNewsArticleAsync(
            UpdateNewsArticleDto updateDto,
            int updatedById
        );
        Task DeleteNewsArticleAsync(int newsArticleId);
        Task UpdateNewsArticleStatusAsync(int newsArticleId, string status, int updatedById);
        Task<BasePaginatedList<NewsArticleDto>> SearchNewsArticlesAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        );
        Task<NewsArticleStatsDto> GetNewsArticleStatsAsync();
    }
}
