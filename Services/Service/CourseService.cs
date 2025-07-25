using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.IRepository.Courses;
using Repositories.IRepository.Users;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.CourseContent;
using Services.DTOs.Courses;
using Services.DTOs.Dashboard;
using Services.DTOs.Registration;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseContentRepository _courseContentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRegistrationRepository _registrationRepository;

        public CourseService(
            IUnitOfWork unitOfWork,
            ICourseRepository courseRepository,
            ICourseContentRepository courseContentRepository,
            IUserRepository userRepository,
            ICourseRegistrationRepository registrationRepository)
        {
            _unitOfWork = unitOfWork;
            _courseRepository = courseRepository;
            _courseContentRepository = courseContentRepository;
            _userRepository = userRepository;
            _registrationRepository = registrationRepository;
        }

        #region Course Management

        public async Task<CourseResponseDto> GetCourseByIdAsync(int courseId)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);

            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            return MapToCourseResponseDto(course);
        }

        public async Task<BasePaginatedList<CourseListDto>> GetCoursesAsync(CourseFilterDto filter)
        {
            var pagedResult = await _courseRepository.GetCoursesWithFiltersAsync(
                searchKeyword: filter.SearchKeyword,
                targetGroup: filter.TargetGroup,
                ageGroup: filter.AgeGroup,
                isActive: filter.IsActive,
                isAccept: filter.IsAccept,
                createdBy: filter.CreatedBy,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                pageIndex: filter.PageIndex,
                pageSize: filter.PageSize);

            var courseDtos = pagedResult.Items.Select(MapToCourseListDto).ToList();

            return new BasePaginatedList<CourseListDto>(
                courseDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize);
        }

        public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto createDto, int createdBy)
        {
            // Validate creator exists
            if (!await _userRepository.ExistsUserAsync(createdBy))
            {
                throw new ArgumentException("Người tạo không tồn tại");
            }

            // Check for duplicate title
            if (await _courseRepository.ExistsCourseByTitleAsync(createDto.Title))
            {
                throw new InvalidOperationException("Tiêu đề khóa học đã tồn tại");
            }

            var course = new Course
            {
                Title = createDto.Title,
                Description = createDto.Description,
                TargetGroup = createDto.TargetGroup,
                AgeGroup = createDto.AgeGroup,
                ContentURL = createDto.ContentURL,
                ThumbnailURL = createDto.ThumbnailURL, // Thêm thuộc tính này
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                isActive = true,
                isAccept = false // Default need approval
            };

            await _courseRepository.InsertAsync(course);
            await _unitOfWork.SaveAsync();

            return await GetCourseByIdAsync(course.CourseID);
        }

        public async Task<CourseResponseDto> UpdateCourseAsync(UpdateCourseDto updateDto, int updatedBy)
        {
            var course = await _courseRepository.GetByIdAsync(updateDto.CourseID);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            // Check for duplicate title (excluding current course)
            if (await _courseRepository.ExistsCourseByTitleAsync(updateDto.Title, updateDto.CourseID))
            {
                throw new InvalidOperationException("Tiêu đề khóa học đã tồn tại");
            }

            // Business rule: Check permission - only creator or admin can update
            // This logic should be enhanced based on your role system

            course.Title = updateDto.Title;
            course.Description = updateDto.Description;
            course.TargetGroup = updateDto.TargetGroup;
            course.AgeGroup = updateDto.AgeGroup;
            course.ContentURL = updateDto.ContentURL;
            course.ThumbnailURL = updateDto.ThumbnailURL; // Thêm thuộc tính này
            course.isActive = updateDto.IsActive;
            course.isAccept = updateDto.IsAccept;

            await _courseRepository.UpdateAsync(course);
            await _unitOfWork.SaveAsync();

            return await GetCourseByIdAsync(course.CourseID);
        }

        public async Task DeleteCourseAsync(int courseId, int deletedBy)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            // Business rule: Check if course has registrations
            if (await _courseRepository.HasRegistrationsAsync(courseId))
            {
                throw new InvalidOperationException("Không thể xóa khóa học đã có người đăng ký");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // SOFT DELETE: Set isActive = false instead of physical delete
                course.isActive = false;
                await _courseRepository.UpdateAsync(course);

                // Also soft delete related contents
                var contents = await _courseContentRepository.GetContentsByCourseOrderedAsync(courseId);
                foreach (var content in contents)
                {
                    content.isActive = false;
                    await _courseContentRepository.UpdateAsync(content);
                }

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ToggleCourseStatusAsync(int courseId, bool isActive, int updatedBy)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            course.isActive = isActive;
            await _courseRepository.UpdateAsync(course);
            await _unitOfWork.SaveAsync();
        }

        public async Task ApproveCourseAsync(int courseId, bool isAccept, int approvedBy)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            course.isAccept = isAccept;
            await _courseRepository.UpdateAsync(course);
            await _unitOfWork.SaveAsync();
        }

        #endregion

        #region Course Content Management

        public async Task<BasePaginatedList<CourseContentResponseDto>> GetCourseContentsAsync(int courseId, int pageIndex = 1, int pageSize = 10)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            var pagedResult = await _courseContentRepository.GetContentsByCourseAsync(courseId, pageIndex, pageSize);

            var contentDtos = pagedResult.Items.Select(MapToCourseContentResponseDto).ToList();

            return new BasePaginatedList<CourseContentResponseDto>(
                contentDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize);
        }

        public async Task<List<CourseContentResponseDto>> GetActiveContentsAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            var contents = await _courseContentRepository.GetActiveContentsByCourseAsync(courseId);
            return contents.Select(MapToCourseContentResponseDto).ToList();
        }

        public async Task<CourseContentResponseDto> GetCourseContentByIdAsync(int contentId)
        {
            var content = await _courseContentRepository.GetContentWithCourseAsync(contentId);

            if (content == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nội dung bài học");
            }

            return MapToCourseContentResponseDto(content);
        }

        public async Task<int> GetNextOrderIndexAsync(int courseId)
        {
            return await _courseContentRepository.GetNextOrderIndexAsync(courseId);
        }

        public async Task<CourseContentResponseDto> CreateCourseContentAsync(CreateCourseContentDto createDto)
        {
            var course = await _courseRepository.GetByIdAsync(createDto.CourseID);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            // Check if OrderIndex already exists
            if (await _courseContentRepository.ExistsContentWithOrderAsync(createDto.CourseID, createDto.OrderIndex))
            {
                throw new InvalidOperationException("Thứ tự này đã tồn tại trong khóa học");
            }

            // Validate content based on type
            ValidateContentData(createDto.ContentType, createDto.ContentData);

            var content = new CourseContent
            {
                CourseID = createDto.CourseID,
                Title = createDto.Title,
                Description = createDto.Description,
                ContentType = createDto.ContentType,
                ContentData = createDto.ContentData,
                OrderIndex = createDto.OrderIndex,
                isActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _courseContentRepository.InsertAsync(content);
            await _unitOfWork.SaveAsync();

            return await GetCourseContentByIdAsync(content.ContentID);
        }

        public async Task<CourseContentResponseDto> UpdateCourseContentAsync(UpdateCourseContentDto updateDto)
        {
            var content = await _courseContentRepository.GetByIdAsync(updateDto.ContentID);
            if (content == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nội dung bài học");
            }

            // Check if new OrderIndex conflicts with existing content
            if (await _courseContentRepository.ExistsContentWithOrderAsync(
                content.CourseID.HasValue ? content.CourseID.Value : 0, updateDto.OrderIndex, updateDto.ContentID))
            {
                throw new InvalidOperationException("Thứ tự này đã tồn tại trong khóa học");
            }

            // Validate content based on type
            ValidateContentData(updateDto.ContentType, updateDto.ContentData);

            content.Title = updateDto.Title;
            content.Description = updateDto.Description;
            content.ContentType = updateDto.ContentType;
            content.ContentData = updateDto.ContentData;
            content.OrderIndex = updateDto.OrderIndex;
            content.isActive = updateDto.IsActive;

            await _courseContentRepository.UpdateAsync(content);
            await _unitOfWork.SaveAsync();

            return await GetCourseContentByIdAsync(content.ContentID);
        }

        public async Task DeleteCourseContentAsync(int contentId)
        {
            var content = await _courseContentRepository.GetByIdAsync(contentId);
            if (content == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nội dung bài học");
            }

            // Business rule: Check if content has progress records
            if (await _courseContentRepository.HasProgressRecordsAsync(contentId))
            {
                throw new InvalidOperationException("Không thể xóa nội dung đã có người học");
            }

            // SOFT DELETE: Set isActive = false instead of physical delete
            content.isActive = false;
            await _courseContentRepository.UpdateAsync(content);
            await _unitOfWork.SaveAsync();
        }

        public async Task ReorderCourseContentsAsync(int courseId, Dictionary<int, int> contentOrderMapping)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var mapping in contentOrderMapping)
                {
                    var content = await _courseContentRepository.GetByIdAsync(mapping.Key);
                    if (content != null && content.CourseID == courseId)
                    {
                        content.OrderIndex = mapping.Value;
                        await _courseContentRepository.UpdateAsync(content);
                    }
                }

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Course Registration Management

        public async Task<CourseRegistrationResponseDto> RegisterForCourseAsync(int courseId, int userId)
        {
            // Validate user exists
            if (!await _userRepository.ExistsUserAsync(userId))
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            // Validate course exists and is available
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            if (!course.isActive || !course.isAccept)
            {
                throw new InvalidOperationException("Khóa học không khả dụng để đăng ký");
            }

            // Check if already registered
            if (await _registrationRepository.IsUserRegisteredAsync(userId, courseId))
            {
                throw new InvalidOperationException("Bạn đã đăng ký khóa học này rồi");
            }

            var registration = new CourseRegistration
            {
                UserID = userId,
                CourseID = courseId,
                RegisteredAt = DateTime.UtcNow,
                Completed = false
            };

            await _registrationRepository.InsertAsync(registration);
            await _unitOfWork.SaveAsync();

            return await GetRegistrationAsync(courseId, userId);
        }

        public async Task UnregisterFromCourseAsync(int courseId, int userId)
        {
            var registration = await _registrationRepository.GetRegistrationAsync(userId, courseId);
            if (registration == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đăng ký khóa học");
            }

            await _registrationRepository.DeleteAsync(registration.RegistrationID);
            await _unitOfWork.SaveAsync();
        }

        public async Task<CourseRegistrationResponseDto> GetRegistrationAsync(int courseId, int userId)
        {
            var registration = await _registrationRepository.GetRegistrationAsync(userId, courseId);
            if (registration == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đăng ký khóa học");
            }

            return MapToCourseRegistrationResponseDto(registration);
        }

        public async Task<BasePaginatedList<RegistrationListDto>> GetCourseRegistrationsAsync(int courseId, RegistrationFilterDto filter)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            filter.CourseID = courseId;
            var registrations = await _registrationRepository.GetCourseRegistrationsAsync(courseId);

            // Apply additional filters
            var filteredRegistrations = ApplyRegistrationFilters(registrations, filter);

            // Apply pagination
            var totalCount = filteredRegistrations.Count();
            var items = filteredRegistrations
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapToRegistrationListDto)
                .ToList();

            return new BasePaginatedList<RegistrationListDto>(
                items, totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<BasePaginatedList<RegistrationListDto>> GetUserRegistrationsAsync(int userId, RegistrationFilterDto filter)
        {
            if (!await _userRepository.ExistsUserAsync(userId))
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            filter.UserID = userId;
            var registrations = await _registrationRepository.GetUserRegistrationsAsync(userId);

            // Apply additional filters
            var filteredRegistrations = ApplyRegistrationFilters(registrations, filter);

            // Apply pagination
            var totalCount = filteredRegistrations.Count();
            var items = filteredRegistrations
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapToRegistrationListDto)
                .ToList();

            return new BasePaginatedList<RegistrationListDto>(
                items, totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<UserLearningDashboardDto> GetUserDashboardAsync(int userId)
        {
            if (!await _userRepository.ExistsUserAsync(userId))
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            var registrations = await _registrationRepository.GetUserRegistrationsAsync(userId);

            var totalRegistrations = registrations.Count;
            var completedCourses = registrations.Count(r => r.Completed);
            var inProgressCourses = registrations.Count(r => !r.Completed);

            return new UserLearningDashboardDto
            {
                UserID = userId,
                UserName = user?.FullName ?? "Không xác định",
                TotalRegistrations = totalRegistrations,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                OverallProgress = totalRegistrations > 0 ? Math.Round((double)completedCourses / totalRegistrations * 100, 2) : 0,
                RecentRegistrations = registrations
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(5)
                    .Select(MapToRegistrationListDto)
                    .ToList(),
                InProgressCoursesList = registrations
                    .Where(r => !r.Completed)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(5)
                    .Select(MapToRegistrationListDto)
                    .ToList()
            };
        }

        public async Task<CourseEnrollmentStatsDto> GetCourseEnrollmentStatsAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Không tìm thấy khóa học");
            }

            var registrations = await _registrationRepository.GetCourseRegistrationsAsync(courseId);

            var totalEnrollments = registrations.Count;
            var completedEnrollments = registrations.Count(r => r.Completed);
            var inProgressEnrollments = registrations.Count(r => !r.Completed);
            var completionRate = totalEnrollments > 0
                ? (double)completedEnrollments / totalEnrollments * 100
                : 0;

            return new CourseEnrollmentStatsDto
            {
                CourseID = courseId,
                CourseTitle = course.Title,
                TotalEnrollments = totalEnrollments,
                CompletedEnrollments = completedEnrollments,
                InProgressEnrollments = inProgressEnrollments,
                CompletionRate = Math.Round(completionRate, 2),
                AverageProgress = Math.Round(completionRate, 2), // Use completion rate as progress since we don't have Progress field
                RecentEnrollments = registrations
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(10)
                    .Select(MapToRegistrationListDto)
                    .ToList()
            };
        }

        public async Task<CourseRegistrationResponseDto> UpdateProgressAsync(UpdateProgressDto updateDto, int userId)
        {
            var registration = await _registrationRepository.GetByIdAsync(updateDto.RegistrationID);
            if (registration == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đăng ký khóa học");
            }

            // Verify user owns this registration
            if (registration.UserID != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đăng ký này");
            }

            registration.Completed = updateDto.Completed;

            await _registrationRepository.UpdateAsync(registration);
            await _unitOfWork.SaveAsync();

            return MapToCourseRegistrationResponseDto(registration);
        }

        public async Task<bool> IsUserRegisteredAsync(int courseId, int userId)
        {
            return await _registrationRepository.IsUserRegisteredAsync(userId, courseId);
        }

        public async Task<bool> CanUserRegisterAsync(int courseId, int userId)
        {
            // Check if user exists
            if (!await _userRepository.ExistsUserAsync(userId))
                return false;

            // Check if course exists and is available
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null || !course.isActive || !course.isAccept)
                return false;

            // Check if already registered
            if (await _registrationRepository.IsUserRegisteredAsync(userId, courseId))
                return false;

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate content data based on content type
        /// </summary>
        private void ValidateContentData(string? contentType, string? contentData)
        {
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(contentData))
                return;

            switch (contentType.ToLower())
            {
                case "video":
                    // Validate video URL
                    if (!IsValidUrl(contentData))
                    {
                        throw new ArgumentException("URL video không hợp lệ");
                    }
                    break;

                case "text":
                    // For text content, ContentData can be HTML or plain text
                    if (contentData.Length > 10000) // Limit text content size
                    {
                        throw new ArgumentException("Nội dung bài viết quá dài (tối đa 10,000 ký tự)");
                    }
                    break;

                case "document":
                    // Validate document URL
                    if (!IsValidUrl(contentData))
                    {
                        throw new ArgumentException("URL tài liệu không hợp lệ");
                    }
                    break;

                case "quiz":
                    // Quiz data should be in JSON format
                    if (!IsValidJson(contentData))
                    {
                        throw new ArgumentException("Dữ liệu quiz phải ở định dạng JSON");
                    }
                    break;

                default:
                    throw new ArgumentException($"Loại nội dung '{contentType}' không được hỗ trợ");
            }
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool IsValidJson(string jsonString)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private IEnumerable<CourseRegistration> ApplyRegistrationFilters(IEnumerable<CourseRegistration> registrations, RegistrationFilterDto filter)
        {
            var query = registrations.AsQueryable();

            if (filter.Completed.HasValue)
            {
                query = query.Where(r => r.Completed == filter.Completed.Value);
            }

            if (!string.IsNullOrEmpty(filter.TargetGroup))
            {
                query = query.Where(r => r.Course != null && r.Course.TargetGroup == filter.TargetGroup);
            }

            if (!string.IsNullOrEmpty(filter.AgeGroup))
            {
                query = query.Where(r => r.Course != null && r.Course.AgeGroup == filter.AgeGroup);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt <= filter.ToDate.Value);
            }

            return query.OrderByDescending(r => r.RegisteredAt);
        }

        #endregion

        #region Private Mapping Methods

        private CourseResponseDto MapToCourseResponseDto(Course course)
        {
            return new CourseResponseDto
            {
                CourseID = course.CourseID,
                Title = course.Title,
                Description = course.Description,
                TargetGroup = course.TargetGroup,
                AgeGroup = course.AgeGroup,
                ContentURL = course.ContentURL,
                ThumbnailURL = course.ThumbnailURL, // Thêm thuộc tính này
                CreatedBy = course.CreatedBy,
                CreatorName = course.Creator?.FullName ?? "Không xác định",
                CreatedAt = course.CreatedAt,
                IsActive = course.isActive,
                IsAccept = course.isAccept,
                TotalContents = course.Contents?.Count ?? 0,
                TotalRegistrations = course.Registrations?.Count ?? 0
            };
        }

        private CourseListDto MapToCourseListDto(Course course)
        {
            return new CourseListDto
            {
                CourseID = course.CourseID,
                Title = course.Title,
                Description = course.Description,
                TargetGroup = course.TargetGroup,
                AgeGroup = course.AgeGroup,
                ThumbnailURL = course.ThumbnailURL, // Thêm thuộc tính này
                CreatorName = course.Creator?.FullName ?? "Không xác định",
                CreatedAt = course.CreatedAt,
                IsActive = course.isActive,
                IsAccept = course.isAccept,
                TotalContents = course.Contents?.Count ?? 0,
                TotalRegistrations = course.Registrations?.Count ?? 0
            };
        }

        private CourseContentResponseDto MapToCourseContentResponseDto(CourseContent content)
        {
            return new CourseContentResponseDto
            {
                ContentID = content.ContentID,
                CourseID = content.CourseID.HasValue ? content.CourseID.Value : 0,
                Title = content.Title,
                Description = content.Description,
                ContentType = content.ContentType,
                ContentData = content.ContentData,
                OrderIndex = content.OrderIndex,
                IsActive = content.isActive,
                CreatedAt = content.CreatedAt,
                CourseName = content.Course?.Title ?? ""
            };
        }

        private CourseRegistrationResponseDto MapToCourseRegistrationResponseDto(CourseRegistration registration)
        {
            return new CourseRegistrationResponseDto
            {
                RegistrationID = registration.RegistrationID,
                UserID = registration.UserID.HasValue ? registration.UserID.Value : 0,
                CourseID = registration.CourseID.HasValue ? registration.CourseID.Value : 0,
                UserName = registration.User?.FullName ?? "Không xác định",
                UserEmail = registration.User?.Email ?? "",
                CourseTitle = registration.Course?.Title ?? "",
                RegisteredAt = registration.RegisteredAt,
                Completed = registration.Completed
            };
        }

        private RegistrationListDto MapToRegistrationListDto(CourseRegistration registration)
        {
            return new RegistrationListDto
            {
                RegistrationID = registration.RegistrationID,
                UserID = registration.UserID.HasValue ? registration.UserID.Value : 0,
                CourseID = registration.CourseID.HasValue ? registration.CourseID.Value : 0,
                UserName = registration.User?.FullName ?? "Không xác định",
                UserEmail = registration.User?.Email ?? "",
                CourseTitle = registration.Course?.Title ?? "",
                TargetGroup = registration.Course?.TargetGroup ?? "",
                AgeGroup = registration.Course?.AgeGroup ?? "",
                RegisteredAt = registration.RegisteredAt,
                Completed = registration.Completed
            };
        }

        #endregion
    }
}