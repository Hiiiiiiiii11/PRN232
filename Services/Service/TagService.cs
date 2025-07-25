using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Tags;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.Tags;
using Services.IService;

namespace Services.Service
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagRepository _tagRepository;

        public TagService(IUnitOfWork unitOfWork, ITagRepository tagRepository)
        {
            _unitOfWork = unitOfWork;
            _tagRepository = tagRepository;
        }

        public async Task<BasePaginatedList<TagDTO>> GetAllTagsAsync(PagingRequest pagingRequest)
        {
            var pagedResult = await _tagRepository.GetTagsWithFiltersAsync(
                searchKeyword: null,
                pageIndex: pagingRequest.index,
                pageSize: pagingRequest.pageSize
            );

            var tagDtos = pagedResult.Items.Select(MapToTagDto).ToList();

            return new BasePaginatedList<TagDTO>(
                tagDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<TagDTO> GetTagByIdAsync(int tagId)
        {
            var tag = await _tagRepository.GetTagWithDetailsAsync(tagId);
            if (tag == null)
                throw new KeyNotFoundException($"Tag với ID {tagId} không tồn tại");

            return MapToTagDto(tag);
        }

        public async Task<TagDTO> AddTagAsync(CreateTagDto createDto)
        {
            // Check for duplicate tag name
            if (await _tagRepository.ExistsTagByNameAsync(createDto.TagName))
            {
                throw new InvalidOperationException("Tên tag đã tồn tại");
            }

            var tag = new Tag { TagName = createDto.TagName, Note = createDto.Note };

            await _tagRepository.InsertAsync(tag);
            await _unitOfWork.SaveAsync();

            return MapToTagDto(tag);
        }

        public async Task<TagDTO> UpdateTagAsync(UpdateTagDto updateDto)
        {
            var existingTag = await _tagRepository.GetByIdAsync(updateDto.TagID);
            if (existingTag == null)
                throw new KeyNotFoundException($"Tag với ID {updateDto.TagID} không tồn tại");

            // Check for duplicate tag name (excluding current tag)
            if (await _tagRepository.ExistsTagByNameAsync(updateDto.TagName, updateDto.TagID))
            {
                throw new InvalidOperationException("Tên tag đã tồn tại");
            }

            existingTag.TagName = updateDto.TagName;
            existingTag.Note = updateDto.Note;

            await _tagRepository.UpdateAsync(existingTag);
            await _unitOfWork.SaveAsync();

            return MapToTagDto(existingTag);
        }

        public async Task DeleteTagAsync(int tagId)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                throw new KeyNotFoundException($"Tag với ID {tagId} không tồn tại");

            // Business rule: Check if tag is being used in news articles
            if (await _tagRepository.IsTagInUseAsync(tagId))
            {
                throw new InvalidOperationException(
                    "Không thể xóa tag đang được sử dụng trong bài viết"
                );
            }

            await _tagRepository.DeleteAsync(tagId);
            await _unitOfWork.SaveAsync();
        }

        public async Task<BasePaginatedList<TagDTO>> SearchTagsAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        )
        {
            var pagedResult = await _tagRepository.GetTagsWithFiltersAsync(
                searchKeyword: searchKeyword,
                pageIndex: pagingRequest.index,
                pageSize: pagingRequest.pageSize
            );

            var tagDtos = pagedResult.Items.Select(MapToTagDto).ToList();

            return new BasePaginatedList<TagDTO>(
                tagDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<List<TagDTO>> GetTopTagsAsync(int count = 10)
        {
            var topTags = await _tagRepository.GetTopTagsByUsageAsync(count);
            return topTags.Select(MapToTagDto).ToList();
        }

        public async Task<List<TagDTO>> GetTagsByNewsArticleAsync(int newsArticleId)
        {
            var tags = await _tagRepository.GetTagsByNewsArticleAsync(newsArticleId);
            return tags.Select(MapToTagDto).ToList();
        }

        #region Private Mapping Methods

        private TagDTO MapToTagDto(Tag tag)
        {
            return new TagDTO
            {
                TagID = tag.TagID,
                TagName = tag.TagName,
                Note = tag.Note,
                //UsageCount = tag.NewsTags?.Count(nt => nt.NewsArticle.NewsStatus == "Active") ?? 0,
            };
        }

        #endregion
    }
}
