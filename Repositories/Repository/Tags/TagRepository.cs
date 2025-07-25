using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Tags;
using Repositories.Paging;

namespace Repositories.Repository.Tags
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<Tag?> GetTagWithDetailsAsync(int tagId)
        {
            return await Entities
                .Include(t => t.NewsTags)
                .ThenInclude(nt => nt.NewsArticle)
                .FirstOrDefaultAsync(t => t.TagID == tagId);
        }

        public async Task<BasePaginatedList<Tag>> GetTagsWithFiltersAsync(
            string? searchKeyword = null,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var query = Entities.Include(t => t.NewsTags).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.Trim().ToLower();
                query = query.Where(t =>
                    t.TagName.ToLower().Contains(keyword)
                    || (t.Note != null && t.Note.ToLower().Contains(keyword))
                );
            }

            // Order by TagName
            query = query.OrderBy(t => t.TagName);

            return await GetPagging(query, pageIndex, pageSize);
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await Entities.Include(t => t.NewsTags).OrderBy(t => t.TagName).ToListAsync();
        }

        public async Task<List<Tag>> GetTagsWithUsageCountAsync()
        {
            return await Entities
                .Include(t => t.NewsTags.Where(nt => nt.NewsArticle.NewsStatus == "Active"))
                .OrderByDescending(t => t.NewsTags.Count)
                .ToListAsync();
        }

        public async Task<bool> ExistsTagByNameAsync(string tagName, int? excludeTagId = null)
        {
            var query = Entities.Where(t => t.TagName.ToLower() == tagName.ToLower());

            if (excludeTagId.HasValue)
            {
                query = query.Where(t => t.TagID != excludeTagId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsTagInUseAsync(int tagId)
        {
            return await Entities
                .Where(t => t.TagID == tagId)
                .SelectMany(t => t.NewsTags)
                .Where(nt => nt.NewsArticle.NewsStatus == "Active")
                .AnyAsync();
        }

        public async Task<Tag?> GetMostUsedTagAsync()
        {
            return await Entities
                .Include(t => t.NewsTags.Where(nt => nt.NewsArticle.NewsStatus == "Active"))
                .OrderByDescending(t => t.NewsTags.Count)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Tag>> GetTopTagsByUsageAsync(int count = 10)
        {
            return await Entities
                .Include(t => t.NewsTags.Where(nt => nt.NewsArticle.NewsStatus == "Active"))
                .OrderByDescending(t => t.NewsTags.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsByNewsArticleAsync(int newsArticleId)
        {
            return await Entities
                .Where(t => t.NewsTags.Any(nt => nt.NewsArticleID == newsArticleId))
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<int> GetUsageCountByTagAsync(int tagId)
        {
            return await Entities
                .Where(t => t.TagID == tagId)
                .SelectMany(t => t.NewsTags)
                .Where(nt => nt.NewsArticle.NewsStatus == "Active")
                .CountAsync();
        }

        public async Task<List<Tag>> SearchTagsByNameAsync(string searchTerm)
        {
            var keyword = searchTerm.Trim().ToLower();
            return await Entities
                .Include(t => t.NewsTags)
                .Where(t => t.TagName.ToLower().Contains(keyword))
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }
    }
}
