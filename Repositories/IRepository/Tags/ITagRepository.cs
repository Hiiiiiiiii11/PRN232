using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.Paging;

namespace Repositories.IRepository.Tags
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        // Tag specific queries - chỉ work với Entity models
        Task<Tag?> GetTagWithDetailsAsync(int tagId);

        Task<BasePaginatedList<Tag>> GetTagsWithFiltersAsync(
            string? searchKeyword = null,
            int pageIndex = 1,
            int pageSize = 10
        );

        Task<List<Tag>> GetAllTagsAsync();

        Task<List<Tag>> GetTagsWithUsageCountAsync();

        Task<bool> ExistsTagByNameAsync(string tagName, int? excludeTagId = null);

        Task<bool> IsTagInUseAsync(int tagId);

        Task<Tag?> GetMostUsedTagAsync();

        Task<List<Tag>> GetTopTagsByUsageAsync(int count = 10);

        Task<List<Tag>> GetTagsByNewsArticleAsync(int newsArticleId);

        Task<int> GetUsageCountByTagAsync(int tagId);

        Task<List<Tag>> SearchTagsByNameAsync(string searchTerm);
    }
}
