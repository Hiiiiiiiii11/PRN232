using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.Common;
using Services.DTOs.NewArticle;
using Services.DTOs.Tags;
using Services.IService;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsArticlesController : ControllerBase
    {
        private readonly INewsArticleService _newsArticleService;

        public NewsArticlesController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        #region ✅ API View NewsArticles - PUBLIC ACCESS (Guest có thể xem)

        /// <summary>
        /// Get all active news articles with pagination and filtering
        /// PUBLIC: Guest, Member đều có thể xem danh sách bài viết
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsArticles(
            [FromQuery] NewsArticleFilterDto filter
        )
        {
            try
            {
                var result = await _newsArticleService.GetAllActiveNewsArticlesAsync(filter);
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get all news articles for admin (including inactive)
        /// ADMIN: Staff, Manager, Admin có thể xem tất cả bài viết
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<
            ActionResult<PaginatedApiResponse<NewsArticleDto>>
        > GetAllNewsArticlesForAdmin([FromQuery] NewsArticleFilterDto filter)
        {
            try
            {
                var result = await _newsArticleService.GetAllNewsArticlesAsync(filter);
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get news article details by ID
        /// PUBLIC: Guest có thể xem chi tiết bài viết
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> GetNewsArticle(int id)
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticleByIdAsync(id);
                return Ok(ApiResponse<NewsArticleDto>.SuccessResult(result));
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
        /// Get news articles by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsByCategory(
            short categoryId,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticlesByCategoryAsync(
                    categoryId,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get news articles by source
        /// </summary>
        [HttpGet("source")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsBySource(
            [FromQuery] string newsSource,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticlesBySourceAsync(
                    newsSource,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Search news articles
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> SearchNewsArticles(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.SearchNewsArticlesAsync(
                    searchKeyword,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Create NewsArticle - Cho Staff, Manager, Admin

        /// <summary>
        /// Create news article
        /// Staff: Quản lý nội dung
        /// Manager: Quản lý tổng thể
        /// Admin: Toàn quyền
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> CreateNewsArticle(
            [FromBody] CreateNewsArticleDto createDto
        )
        {
            try
            {
                short createdBy = GetCurrentUserId();
                var result = await _newsArticleService.AddNewsArticleAsync(createDto, createdBy);
                return CreatedAtAction(
                    nameof(GetNewsArticle),
                    new { id = result.NewsArticleID },
                    ApiResponse<NewsArticleDto>.SuccessResult(result, "Tạo bài viết thành công")
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo bài viết")
                );
            }
        }

        #endregion

        #region ✅ API Update/Delete NewsArticle - Cho Staff, Manager, Admin

        /// <summary>
        /// Update news article
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> UpdateNewsArticle(
            int id,
            [FromBody] UpdateNewsArticleDto updateDto
        )
        {
            try
            {
                if (id != updateDto.NewsArticleID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                short updatedBy = GetCurrentUserId();
                var result = await _newsArticleService.UpdateNewsArticleAsync(updateDto, updatedBy);
                return Ok(
                    ApiResponse<NewsArticleDto>.SuccessResult(
                        result,
                        "Cập nhật bài viết thành công"
                    )
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
        /// Delete news article (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteNewsArticle(int id)
        {
            try
            {
                await _newsArticleService.DeleteNewsArticleAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa bài viết thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Toggle news article status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleNewsStatus(
            int id,
            [FromBody] string isActive
        )
        {
            try
            {
                short updatedBy = GetCurrentUserId();
                await _newsArticleService.UpdateNewsArticleStatusAsync(id, isActive, updatedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật trạng thái thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get news article statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleStatsDto>>> GetNewsArticleStats()
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticleStatsAsync();
                return Ok(ApiResponse<NewsArticleStatsDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Helper Methods

        private short GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && short.TryParse(userIdClaim.Value, out short userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }

        #endregion
    }
}
