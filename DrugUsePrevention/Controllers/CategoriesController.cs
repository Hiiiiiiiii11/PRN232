using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.Common;
using Services.IService;

namespace DrugUsePrevention.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        #region ✅ API View Categories - PUBLIC ACCESS

        /// <summary>
        /// Get all active categories
        /// PUBLIC: Guest có thể xem danh mục
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> GetCategories(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.GetAllActiveCategoriesAsync(pagingRequest);
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get all categories for admin (including inactive)
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> GetAllCategoriesForAdmin(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync(pagingRequest);
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetCategory(short id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(ApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Search categories
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> SearchCategories(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.SearchCategoriesAsync(
                    searchKeyword,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Manage Categories - Admin Only

        /// <summary>
        /// Create category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> CreateCategory(
            [FromBody] CreateCategoryDto createDto
        )
        {
            try
            {
                var result = await _categoryService.AddCategoryAsync(createDto);
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = result.CategoryID },
                    ApiResponse<CategoryDTO>.SuccessResult(result, "Tạo danh mục thành công")
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> UpdateCategory(
            short id,
            [FromBody] UpdateCategoryDto updateDto
        )
        {
            try
            {
                if (id != updateDto.CategoryID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _categoryService.UpdateCategoryAsync(updateDto);
                return Ok(
                    ApiResponse<CategoryDTO>.SuccessResult(result, "Cập nhật danh mục thành công")
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(short id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa danh mục thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Get category statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryStatsDto>>> GetCategoryStats()
        {
            try
            {
                var result = await _categoryService.GetCategoryStatsAsync();
                return Ok(ApiResponse<CategoryStatsDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion
    }
}
