using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.Tags;

namespace Services.IService
{
    public interface ITagService
    {
        Task<BasePaginatedList<TagDTO>> GetAllTagsAsync(PagingRequest pagingRequest);
        Task<TagDTO> GetTagByIdAsync(int tagId);
        Task<TagDTO> AddTagAsync(CreateTagDto createDto);
        Task<TagDTO> UpdateTagAsync(UpdateTagDto updateDto);
        Task DeleteTagAsync(int tagId);
        Task<BasePaginatedList<TagDTO>> SearchTagsAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        );
        Task<List<TagDTO>> GetTopTagsAsync(int count = 10);
        Task<List<TagDTO>> GetTagsByNewsArticleAsync(int newsArticleId);
    }
}
