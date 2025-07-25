using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.DTOs.Common;
using Services.DTOs.Tags;
using Services.IService;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // ==============================================
    // TAGS CONTROLLER
    // ==============================================
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        #region ✅ API View Tags - PUBLIC ACCESS

        /// <summary>
        /// Get all tags
        /// PUBLIC: Guest có thể xem tags
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<TagDTO>>> GetTags(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _tagService.GetAllTagsAsync(pagingRequest);
                return Ok(PaginatedApiResponse<TagDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TagDTO>>> GetTag(int id)
        {
            try
            {
                var result = await _tagService.GetTagByIdAsync(id);
                return Ok(ApiResponse<TagDTO>.SuccessResult(result));
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
        /// Search tags
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<TagDTO>>> SearchTags(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _tagService.SearchTagsAsync(searchKeyword, pagingRequest);
                return Ok(PaginatedApiResponse<TagDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get top tags by usage
        /// </summary>
        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<TagDTO>>>> GetTopTags(
            [FromQuery] int count = 10
        )
        {
            try
            {
                var result = await _tagService.GetTopTagsAsync(count);
                return Ok(ApiResponse<List<TagDTO>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get tags by news article
        /// </summary>
        [HttpGet("news/{newsId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<TagDTO>>>> GetTagsByNewsArticle(int newsId)
        {
            try
            {
                var result = await _tagService.GetTagsByNewsArticleAsync(newsId);
                return Ok(ApiResponse<List<TagDTO>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Manage Tags - Staff, Manager, Admin

        /// <summary>
        /// Create tag
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<TagDTO>>> CreateTag(
            [FromBody] CreateTagDto createDto
        )
        {
            try
            {
                var result = await _tagService.AddTagAsync(createDto);
                return CreatedAtAction(
                    nameof(GetTag),
                    new { id = result.TagID },
                    ApiResponse<TagDTO>.SuccessResult(result, "Tạo tag thành công")
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
        /// Update tag
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<TagDTO>>> UpdateTag(
            int id,
            [FromBody] UpdateTagDto updateDto
        )
        {
            try
            {
                if (id != updateDto.TagID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _tagService.UpdateTagAsync(updateDto);
                return Ok(ApiResponse<TagDTO>.SuccessResult(result, "Cập nhật tag thành công"));
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
        /// Delete tag
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTag(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa tag thành công"));
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

        #endregion
    }
}
