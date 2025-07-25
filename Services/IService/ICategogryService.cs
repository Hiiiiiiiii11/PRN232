using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Category;

namespace Services.IService
{
    public interface ICategoryService
    {
        Task<BasePaginatedList<CategoryDTO>> GetAllCategoriesAsync(PagingRequest pagingRequest);
        Task<BasePaginatedList<CategoryDTO>> GetAllActiveCategoriesAsync(
            PagingRequest pagingRequest
        );
        Task<CategoryDTO> GetCategoryByIdAsync(int categoryId);
        Task<CategoryDTO> AddCategoryAsync(CreateCategoryDto createDto);
        Task<CategoryDTO> UpdateCategoryAsync(UpdateCategoryDto updateDto);
        Task DeleteCategoryAsync(int categoryId);
        Task<BasePaginatedList<CategoryDTO>> SearchCategoriesAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        );
        Task<CategoryStatsDto> GetCategoryStatsAsync();
    }
}
