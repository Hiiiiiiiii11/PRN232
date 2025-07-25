using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.NewsArticles;
using Repositories.IRepository.Users;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.NewArticle;
using Services.DTOs.Tags;
using Services.DTOs.User;
using Services.IService;

namespace Services.Service
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly IUserRepository _userRepository;

        public NewsArticleService(
            IUnitOfWork unitOfWork,
            INewsArticleRepository newsArticleRepository,
            IUserRepository userRepository
        )
        {
            _unitOfWork = unitOfWork;
            _newsArticleRepository = newsArticleRepository;
            _userRepository = userRepository;
        }

        public async Task<BasePaginatedList<NewsArticleDto>> GetAllNewsArticlesAsync(
            NewsArticleFilterDto filter
        )
        {
            var pagedResult = await _newsArticleRepository.GetNewsArticlesWithFiltersAsync(
                searchKeyword: filter.SearchKeyword,
                categoryId: filter.CategoryID,
                newsSource: filter.NewsSource,
                newsStatus: filter.NewsStatus ?? "Active",
                createdBy: filter.CreatedByID,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                pageIndex: filter.PageIndex,
                pageSize: filter.PageSize
            );

            var newsArticleDtos = pagedResult
                .Items.Select(newsArticle => new NewsArticleDto
                {
                    NewsArticleID = newsArticle.NewsArticleID,
                    NewsTitle = newsArticle.NewsAticleName,
                    Headline = newsArticle.Headline,
                    CreatedDate = newsArticle.CreatedDate,
                    NewsContent = newsArticle.NewsContent,
                    NewsSource = newsArticle.NewsSource,
                    CategoryID = newsArticle.CategoryID,
                    NewsStatus = newsArticle.NewsStatus,
                    CreatedByID = newsArticle.CreatedByID,
                    UpdatedByID = newsArticle.UpdatedByID,
                    ModifiedDate = newsArticle.ModifiedDate ?? DateTime.Now,
                    CreatedBy =
                        newsArticle.CreatedBy != null
                            ? new UserDTO
                            {
                                UserID = newsArticle.CreatedBy.UserID,
                                FullName = newsArticle.CreatedBy.FullName,
                                Username = newsArticle.CreatedBy.Username,
                                Email = newsArticle.CreatedBy.Email,
                                Role = newsArticle.CreatedBy.Role,
                            }
                            : null,
                    UpdatedBy =
                        newsArticle.UpdatedBy != null
                            ? new UserDTO
                            {
                                UserID = newsArticle.UpdatedBy.UserID,
                                FullName = newsArticle.UpdatedBy.FullName,
                                Username = newsArticle.UpdatedBy.Username,
                                Email = newsArticle.UpdatedBy.Email,
                                Role = newsArticle.UpdatedBy.Role,
                            }
                            : null,
                    Category =
                        newsArticle.Category != null
                            ? new CategoryDTO
                            {
                                CategoryID = newsArticle.Category.CategoryID,
                                CategoryName = newsArticle.Category.CategoryName,
                                CategoryDescription = newsArticle.Category.CategoryDescription,
                            }
                            : null,
                    NewsTags = newsArticle
                        .NewsTags?.Select(nt => new NewsTagDTO
                        {
                            NewsArticleID = nt.NewsArticleID,
                            TagID = nt.TagID,
                            Tag =
                                nt.Tag != null
                                    ? new TagDTO
                                    {
                                        TagID = nt.Tag.TagID,
                                        TagName = nt.Tag.TagName,
                                        Note = nt.Tag.Note,
                                    }
                                    : null,
                        })
                        .ToList(),
                })
                .ToList();

            return new BasePaginatedList<NewsArticleDto>(
                newsArticleDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<BasePaginatedList<NewsArticleDto>> GetAllActiveNewsArticlesAsync(
            NewsArticleFilterDto filter
        )
        {
            filter.NewsStatus = "Active"; // Only active news
            return await GetAllNewsArticlesAsync(filter);
        }

        public async Task<NewsArticleDto> GetNewsArticleByIdAsync(int newsArticleId)
        {
            var newsArticle = await _newsArticleRepository.GetNewsArticleWithDetailsAsync(
                newsArticleId
            );
            if (newsArticle == null)
                throw new KeyNotFoundException($"Bài viết với ID {newsArticleId} không tồn tại");

            return new NewsArticleDto
            {
                NewsArticleID = newsArticle.NewsArticleID,
                NewsTitle = newsArticle.NewsAticleName,
                Headline = newsArticle.Headline,
                CreatedDate = newsArticle.CreatedDate,
                NewsContent = newsArticle.NewsContent,
                NewsSource = newsArticle.NewsSource,
                CategoryID = newsArticle.CategoryID,
                NewsStatus = newsArticle.NewsStatus,
                CreatedByID = newsArticle.CreatedByID,
                UpdatedByID = newsArticle.UpdatedByID,
                ModifiedDate = newsArticle.ModifiedDate ?? DateTime.Now,
                CreatedBy =
                    newsArticle.CreatedBy != null
                        ? new UserDTO
                        {
                            UserID = newsArticle.CreatedBy.UserID,
                            FullName = newsArticle.CreatedBy.FullName,
                            Username = newsArticle.CreatedBy.Username,
                            Email = newsArticle.CreatedBy.Email,
                            Role = newsArticle.CreatedBy.Role,
                        }
                        : null,
                UpdatedBy =
                    newsArticle.UpdatedBy != null
                        ? new UserDTO
                        {
                            UserID = newsArticle.UpdatedBy.UserID,
                            FullName = newsArticle.UpdatedBy.FullName,
                            Username = newsArticle.UpdatedBy.Username,
                            Email = newsArticle.UpdatedBy.Email,
                            Role = newsArticle.UpdatedBy.Role,
                        }
                        : null,
                Category =
                    newsArticle.Category != null
                        ? new CategoryDTO
                        {
                            CategoryID = newsArticle.Category.CategoryID,
                            CategoryName = newsArticle.Category.CategoryName,
                            CategoryDescription = newsArticle.Category.CategoryDescription,
                        }
                        : null,
                NewsTags = newsArticle
                    .NewsTags?.Select(nt => new NewsTagDTO
                    {
                        NewsArticleID = nt.NewsArticleID,
                        TagID = nt.TagID,
                        Tag =
                            nt.Tag != null
                                ? new TagDTO
                                {
                                    TagID = nt.Tag.TagID,
                                    TagName = nt.Tag.TagName,
                                    Note = nt.Tag.Note,
                                }
                                : null,
                    })
                    .ToList(),
            };
        }

        public async Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesByCategoryAsync(
            int categoryId,
            PagingRequest pagingRequest
        )
        {
            var filter = new NewsArticleFilterDto
            {
                CategoryID = categoryId,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                NewsStatus =
                    "Active" // Only active news for public view
                ,
            };

            return await GetAllNewsArticlesAsync(filter);
        }

        public async Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesBySourceAsync(
            string newsSource,
            PagingRequest pagingRequest
        )
        {
            var filter = new NewsArticleFilterDto
            {
                NewsSource = newsSource,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                NewsStatus =
                    "Active" // Only active news for public view
                ,
            };

            return await GetAllNewsArticlesAsync(filter);
        }

        public async Task<BasePaginatedList<NewsArticleDto>> GetNewsArticlesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            PagingRequest pagingRequest
        )
        {
            var filter = new NewsArticleFilterDto
            {
                FromDate = startDate,
                ToDate = endDate,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                NewsStatus =
                    "Active" // Only active news for public view
                ,
            };

            return await GetAllNewsArticlesAsync(filter);
        }

        public async Task<NewsArticleDto> AddNewsArticleAsync(
            CreateNewsArticleDto createDto,
            int createdById
        )
        {
            // Validate creator exists
            if (!await _userRepository.ExistsUserAsync(createdById))
            {
                throw new ArgumentException("Người tạo không tồn tại");
            }

            // Check for duplicate title
            if (await _newsArticleRepository.ExistsNewsByTitleAsync(createDto.NewsTitle))
            {
                throw new InvalidOperationException("Tiêu đề bài viết đã tồn tại");
            }

            var newsArticle = new NewsArticle
            {
                //NewsArticleID = int.Newint(),
                NewsAticleName = createDto.NewsTitle,
                Headline = createDto.Headline,
                NewsContent = createDto.NewsContent,
                NewsSource = createDto.NewsSource,
                CategoryID = createDto.CategoryID,
                NewsStatus = createDto.NewsStatus ?? "Active",
                CreatedByID = createdById,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };

            await _newsArticleRepository.InsertAsync(newsArticle);
            await _unitOfWork.SaveAsync();

            return await GetNewsArticleByIdAsync(newsArticle.NewsArticleID);
        }

        public async Task<NewsArticleDto> UpdateNewsArticleAsync(
            UpdateNewsArticleDto updateDto,
            int updatedById
        )
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(updateDto.NewsArticleID);
            if (newsArticle == null)
                throw new KeyNotFoundException("Không tìm thấy bài viết");

            // Check for duplicate title (excluding current article)
            if (
                await _newsArticleRepository.ExistsNewsByTitleAsync(
                    updateDto.NewsTitle,
                    updateDto.NewsArticleID
                )
            )
            {
                throw new InvalidOperationException("Tiêu đề bài viết đã tồn tại");
            }

            newsArticle.NewsAticleName = updateDto.NewsTitle;
            newsArticle.Headline = updateDto.Headline;
            newsArticle.NewsContent = updateDto.NewsContent;
            newsArticle.NewsSource = updateDto.NewsSource;
            newsArticle.CategoryID = updateDto.CategoryID;
            newsArticle.NewsStatus = updateDto.NewsStatus;
            newsArticle.UpdatedByID = updatedById;
            newsArticle.ModifiedDate = DateTime.UtcNow;

            await _newsArticleRepository.UpdateAsync(newsArticle);
            await _unitOfWork.SaveAsync();

            return await GetNewsArticleByIdAsync(newsArticle.NewsArticleID);
        }

        public async Task DeleteNewsArticleAsync(int newsArticleId)
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException("Không tìm thấy bài viết");

            // SOFT DELETE: Set NewsStatus = false instead of physical delete
            newsArticle.NewsStatus = "Inactive";
            newsArticle.ModifiedDate = DateTime.UtcNow;

            await _newsArticleRepository.UpdateAsync(newsArticle);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateNewsArticleStatusAsync(
            int newsArticleId,
            string status,
            int updatedById
        )
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException("Không tìm thấy bài viết");

            newsArticle.NewsStatus = status;
            newsArticle.UpdatedByID = updatedById;
            newsArticle.ModifiedDate = DateTime.UtcNow;

            await _newsArticleRepository.UpdateAsync(newsArticle);
            await _unitOfWork.SaveAsync();
        }

        public async Task<BasePaginatedList<NewsArticleDto>> SearchNewsArticlesAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        )
        {
            var filter = new NewsArticleFilterDto
            {
                SearchKeyword = searchKeyword,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                NewsStatus =
                    "Active" // Only active news for public search
                ,
            };

            return await GetAllNewsArticlesAsync(filter);
        }

        public async Task<NewsArticleStatsDto> GetNewsArticleStatsAsync()
        {
            var allNews = await _newsArticleRepository.GetAllAsync();

            var totalArticles = allNews.Count();
            var activeArticles = allNews.Count(n => n.NewsStatus == "Active");
            var inactiveArticles = allNews.Count(n => n.NewsStatus == "Inactive");
            var todayArticles = allNews.Count(n => n.CreatedDate.Date == DateTime.Today);
            var thisWeekArticles = allNews.Count(n => n.CreatedDate >= DateTime.Today.AddDays(-7));
            var thisMonthArticles = allNews.Count(n =>
                n.CreatedDate >= DateTime.Today.AddMonths(-1)
            );

            return new NewsArticleStatsDto
            {
                TotalArticles = totalArticles,
                ActiveArticles = activeArticles,
                InactiveArticles = inactiveArticles,
                TodayArticles = todayArticles,
                ThisWeekArticles = thisWeekArticles,
                ThisMonthArticles = thisMonthArticles,
                RecentArticles = allNews
                    .Where(n => n.NewsStatus == "Active")
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(5)
                    .Select(newsArticle => new NewsArticleDto
                    {
                        NewsArticleID = newsArticle.NewsArticleID,
                        NewsTitle = newsArticle.NewsAticleName,
                        Headline = newsArticle.Headline,
                        CreatedDate = newsArticle.CreatedDate,
                        NewsContent = newsArticle.NewsContent,
                        NewsSource = newsArticle.NewsSource,
                        CategoryID = newsArticle.CategoryID,
                        NewsStatus = newsArticle.NewsStatus,
                        CreatedByID = newsArticle.CreatedByID,
                        UpdatedByID = newsArticle.UpdatedByID,
                        ModifiedDate = newsArticle.ModifiedDate ?? DateTime.Now,
                        CreatedBy =
                            newsArticle.CreatedBy != null
                                ? new UserDTO
                                {
                                    UserID = newsArticle.CreatedBy.UserID,
                                    FullName = newsArticle.CreatedBy.FullName,
                                    Username = newsArticle.CreatedBy.Username,
                                    Email = newsArticle.CreatedBy.Email,
                                    Role = newsArticle.CreatedBy.Role,
                                }
                                : null,
                        UpdatedBy =
                            newsArticle.UpdatedBy != null
                                ? new UserDTO
                                {
                                    UserID = newsArticle.UpdatedBy.UserID,
                                    FullName = newsArticle.UpdatedBy.FullName,
                                    Username = newsArticle.UpdatedBy.Username,
                                    Email = newsArticle.UpdatedBy.Email,
                                    Role = newsArticle.UpdatedBy.Role,
                                }
                                : null,
                        Category =
                            newsArticle.Category != null
                                ? new CategoryDTO
                                {
                                    CategoryID = newsArticle.Category.CategoryID,
                                    CategoryName = newsArticle.Category.CategoryName,
                                    CategoryDescription = newsArticle.Category.CategoryDescription,
                                }
                                : null,
                        NewsTags = newsArticle
                            .NewsTags?.Select(nt => new NewsTagDTO
                            {
                                NewsArticleID = nt.NewsArticleID,
                                TagID = nt.TagID,
                                Tag =
                                    nt.Tag != null
                                        ? new TagDTO
                                        {
                                            TagID = nt.Tag.TagID,
                                            TagName = nt.Tag.TagName,
                                            Note = nt.Tag.Note,
                                        }
                                        : null,
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }
    }
}
