using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Categories;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Category;
using Services.IService;

namespace Services.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(IUnitOfWork unitOfWork, ICategoryRepository categoryRepository)
        {
            _unitOfWork = unitOfWork;
            _categoryRepository = categoryRepository;
        }

        public async Task<BasePaginatedList<CategoryDTO>> GetAllCategoriesAsync(
            PagingRequest pagingRequest
        )
        {
            var pagedResult = await _categoryRepository.GetCategoriesWithFiltersAsync(
                searchKeyword: null,
                pageIndex: pagingRequest.index,
                pageSize: pagingRequest.pageSize
            );

            var categoryDtos = pagedResult
                .Items.Select(c => new CategoryDTO
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription,
                    NewsCount = c.NewsAticles?.Count ?? 0,
                })
                .ToList();

            return new BasePaginatedList<CategoryDTO>(
                categoryDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<BasePaginatedList<CategoryDTO>> GetAllActiveCategoriesAsync(
            PagingRequest pagingRequest
        )
        {
            var pagedResult = await _categoryRepository.GetCategoriesWithFiltersAsync(
                searchKeyword: null,
                pageIndex: pagingRequest.index,
                pageSize: pagingRequest.pageSize
            );

            var categoryDtos = pagedResult
                .Items.Select(c => new CategoryDTO
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription,
                    NewsCount = c.NewsAticles?.Count ?? 0,
                })
                .ToList();

            return new BasePaginatedList<CategoryDTO>(
                categoryDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int categoryId)
        {
            var c = await _categoryRepository.GetCategoryWithDetailsAsync(categoryId);
            if (c == null)
                throw new KeyNotFoundException($"Category với ID {categoryId} không tồn tại");

            return new CategoryDTO
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription,
                NewsCount = c.NewsAticles?.Count ?? 0,
            };
        }

        public async Task<CategoryDTO> AddCategoryAsync(CreateCategoryDto createDto)
        {
            // Check for duplicate category name
            if (await _categoryRepository.ExistsCategoryByNameAsync(createDto.CategoryName))
            {
                throw new InvalidOperationException("Tên danh mục đã tồn tại");
            }

            var category = new Category
            {
                CategoryName = createDto.CategoryName,
                CategoryDescription = createDto.Description,
            };

            await _categoryRepository.InsertAsync(category);
            await _unitOfWork.SaveAsync();

            return new CategoryDTO
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                NewsCount = category.NewsAticles?.Count ?? 0,
            };
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(UpdateCategoryDto updateDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(updateDto.CategoryID);
            if (existingCategory == null)
                throw new KeyNotFoundException(
                    $"Category với ID {updateDto.CategoryID} không tồn tại"
                );

            // Check for duplicate category name (excluding current category)
            if (
                await _categoryRepository.ExistsCategoryByNameAsync(
                    updateDto.CategoryName,
                    updateDto.CategoryID
                )
            )
            {
                throw new InvalidOperationException("Tên danh mục đã tồn tại");
            }

            existingCategory.CategoryName = updateDto.CategoryName;
            existingCategory.CategoryDescription = updateDto.Description;

            await _categoryRepository.UpdateAsync(existingCategory);
            await _unitOfWork.SaveAsync();

            return new CategoryDTO
            {
                CategoryID = existingCategory.CategoryID,
                CategoryName = existingCategory.CategoryName,
                CategoryDescription = existingCategory.CategoryDescription,
                NewsCount = existingCategory.NewsAticles?.Count ?? 0,
            };
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category với ID {categoryId} không tồn tại");

            // Business rule: Check if category is being used in news articles

            // SOFT DELETE: Set CategoryStatus = false instead of physical delete
            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCategoryStatusAsync(short categoryId, bool status)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category với ID {categoryId} không tồn tại");

            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<BasePaginatedList<CategoryDTO>> SearchCategoriesAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        )
        {
            var pagedResult = await _categoryRepository.GetCategoriesWithFiltersAsync(
                searchKeyword: searchKeyword,
                pageIndex: pagingRequest.index,
                pageSize: pagingRequest.pageSize
            );

            var categoryDtos = pagedResult
                .Items.Select(c => new CategoryDTO
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription,
                    NewsCount = c.NewsAticles?.Count ?? 0,
                })
                .ToList();

            return new BasePaginatedList<CategoryDTO>(
                categoryDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<CategoryStatsDto> GetCategoryStatsAsync()
        {
            var allCategories = await _categoryRepository.GetAllAsync();

            var totalCategories = allCategories.Count();

            return new CategoryStatsDto
            {
                TotalCategories = totalCategories,
                MostUsedCategories = allCategories
                    .OrderByDescending(c => c.NewsAticles?.Count ?? 0)
                    .Take(5)
                    .Select(c => new CategoryDTO
                    {
                        CategoryID = c.CategoryID,
                        CategoryName = c.CategoryName,
                        CategoryDescription = c.CategoryDescription,
                        NewsCount = c.NewsAticles?.Count ?? 0,
                    })
                    .ToList(),
            };
        }
    }
}
