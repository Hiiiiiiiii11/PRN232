using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.ConsultantUser;
using Services.DTOs.Courses;
using Services.DTOs.User;
using Services.IService;

namespace Services.Service
{
    public class ConsultantUserService : IConsultantUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConsultantUserRepository _consultantRepository;
        private readonly IUserRepository _userRepository;

        public ConsultantUserService(
            IUnitOfWork unitOfWork,
            IConsultantUserRepository consultantRepository,
            IUserRepository userRepository
        )
        {
            _unitOfWork = unitOfWork;
            _consultantRepository = consultantRepository;
            _userRepository = userRepository;
        }

        public async Task<BasePaginatedList<ConsultantDto>> GetAllConsultantsAsync(
            ConsultantFilterDto filter
        )
        {
            var pagedResult = await _consultantRepository.GetConsultantsWithFiltersAsync(
                searchKeyword: filter.SearchKeyword,
                specialty: filter.Specialty,
                status: filter.Status ?? "Active",
                pageIndex: filter.PageIndex,
                pageSize: filter.PageSize
            );

            var consultantDtos = pagedResult
                .Items.Select(consultant => new ConsultantDto
                {
                    ConsultantID = consultant.ConsultantID,
                    Qualifications = consultant.Qualifications,
                    Specialty = consultant.Specialty,
                    WorkingHours = consultant.WorkingHours ?? new List<DateTime>(),
                    User =
                        consultant.User != null
                            ? new UserDTO
                            {
                                UserID = consultant.User.UserID,
                                FullName = consultant.User.FullName,
                                Username = consultant.User.Username,
                                Email = consultant.User.Email,
                                Role = consultant.User.Role,
                                Phone = consultant.User.Phone,
                                AvatarUrl = consultant.User.AvatarUrl,
                                Status = consultant.User.Status,
                                DateOfBirth = consultant.User.DateOfBirth ?? DateTime.MinValue,
                                CreatedAt = consultant.User.CreatedAt,
                            }
                            : null,
                    CreatedCourses =
                        consultant
                            .User?.CreatedCourses?.Select(course => new CourseDto
                            {
                                CourseID = course.CourseID,
                                CourseName = course.Title,
                                CourseDescription = course.Description,
                                Status = course.isActive == true ? "Active" : "Inactive",
                                CreatedByID = course.CreatedBy,
                                CreatedAt = course.CreatedAt,
                                ThumbnailURL = course.ThumbnailURL,
                            })
                            .ToList() ?? new List<CourseDto>(),
                })
                .ToList();

            return new BasePaginatedList<ConsultantDto>(
                consultantDtos,
                pagedResult.TotalItems,
                pagedResult.CurrentPage,
                pagedResult.PageSize
            );
        }

        public async Task<BasePaginatedList<ConsultantDto>> GetAllActiveConsultantsAsync(
            ConsultantFilterDto filter
        )
        {
            filter.Status = "Active"; // Only active consultants
            return await GetAllConsultantsAsync(filter);
        }

        public async Task<ConsultantDto> GetConsultantByIdAsync(int consultantId)
        {
            var consultant = await _consultantRepository.GetConsultantWithDetailsAsync(
                consultantId
            );
            if (consultant == null)
                throw new KeyNotFoundException($"Consultant với ID {consultantId} không tồn tại");

            return new ConsultantDto
            {
                ConsultantID = consultant.ConsultantID,
                Qualifications = consultant.Qualifications,
                Specialty = consultant.Specialty,
                WorkingHours = consultant.WorkingHours ?? new List<DateTime>(),
                User =
                    consultant.User != null
                        ? new UserDTO
                        {
                            UserID = consultant.User.UserID,
                            FullName = consultant.User.FullName,
                            Username = consultant.User.Username,
                            Email = consultant.User.Email,
                        }
                        : null,
                CreatedCourses =
                    consultant
                        .User?.CreatedCourses?.Select(course => new CourseDto
                        {
                            CourseID = course.CourseID,
                            CourseName = course.Title,
                            CourseDescription = course.Description,
                            Status = course.isActive == true ? "Active" : "Inactive",
                            CreatedByID = course.CreatedBy,
                            CreatedAt = course.CreatedAt,
                        })
                        .ToList() ?? new List<CourseDto>(),
            };
        }

        public async Task<ConsultantDto> GetConsultantByUserIdAsync(int userId)
        {
            var consultant = await _consultantRepository.GetConsultantByUserIdAsync(userId);
            if (consultant == null)
                throw new KeyNotFoundException($"Consultant với User ID {userId} không tồn tại");

            return await GetConsultantByIdAsync(consultant.ConsultantID);
        }

        public async Task<BasePaginatedList<CourseDto>> GetConsultantCoursesAsync(
            int consultantId,
            PagingRequest pagingRequest
        )
        {
            var pagedCourses = await _consultantRepository.GetConsultantCoursesPagedAsync(
                consultantId,
                pagingRequest.index,
                pagingRequest.pageSize
            );

            var courseDtos = pagedCourses
                .Items.Select(course => new CourseDto
                {
                    CourseID = course.CourseID,
                    CourseName = course.Title,
                    CourseDescription = course.Description,
                    Status = course.isActive == true ? "Active" : "Inactive",
                    CreatedByID = course.CreatedBy,
                    CreatedAt = course.CreatedAt,
                })
                .ToList();

            return new BasePaginatedList<CourseDto>(
                courseDtos,
                pagedCourses.TotalItems,
                pagedCourses.CurrentPage,
                pagedCourses.PageSize
            );
        }

        public async Task<BasePaginatedList<ConsultantDto>> SearchConsultantsAsync(
            string searchKeyword,
            PagingRequest pagingRequest
        )
        {
            var filter = new ConsultantFilterDto
            {
                SearchKeyword = searchKeyword,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                Status =
                    "Active" // Only active consultants for public search
                ,
            };

            return await GetAllConsultantsAsync(filter);
        }

        public async Task<BasePaginatedList<ConsultantDto>> GetConsultantsBySpecialtyAsync(
            string specialty,
            PagingRequest pagingRequest
        )
        {
            var filter = new ConsultantFilterDto
            {
                Specialty = specialty,
                PageIndex = pagingRequest.index,
                PageSize = pagingRequest.pageSize,
                Status =
                    "Active" // Only active consultants for public view
                ,
            };

            return await GetAllConsultantsAsync(filter);
        }

        public async Task DeleteConsultantAsync(int consultantId)
        {
            var consultant = await _consultantRepository.GetByIdAsync(consultantId);
            if (consultant == null)
                throw new KeyNotFoundException("Không tìm thấy consultant");

            // Check if consultant is referenced by other entities
            if (await _consultantRepository.IsConsultantReferencedAsync(consultantId))
            {
                throw new InvalidOperationException(
                    "Không thể xóa consultant vì đang được tham chiếu bởi các thực thể khác"
                );
            }

            // SOFT DELETE: Update user status instead of physical delete
            if (consultant.User != null)
            {
                consultant.User.Status = "Inactive";
                await _userRepository.UpdateAsync(consultant.User);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateConsultantStatusAsync(
            int consultantId,
            string status,
            int updatedById
        )
        {
            var consultant = await _consultantRepository.GetConsultantWithDetailsAsync(
                consultantId
            );
            if (consultant == null)
                throw new KeyNotFoundException("Không tìm thấy consultant");

            if (consultant.User != null)
            {
                consultant.User.Status = status;
                await _userRepository.UpdateAsync(consultant.User);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<ConsultantStatsDto> GetConsultantStatsAsync()
        {
            var totalConsultants = await _consultantRepository.CountAsync();
            var activeConsultants =
                await _consultantRepository.GetTotalActiveConsultantsCountAsync();
            var inactiveConsultants =
                await _consultantRepository.GetTotalInactiveConsultantsCountAsync();
            var totalCourses = await _consultantRepository.GetTotalCoursesCountAsync();
            var recentConsultants = await _consultantRepository.GetRecentConsultantsAsync(5);

            return new ConsultantStatsDto
            {
                TotalConsultants = totalConsultants,
                ActiveConsultants = activeConsultants,
                InactiveConsultants = inactiveConsultants,
                TotalCourses = totalCourses,
                RecentConsultants = recentConsultants
                    .Select(consultant => new ConsultantDto
                    {
                        ConsultantID = consultant.ConsultantID,
                        Qualifications = consultant.Qualifications,
                        Specialty = consultant.Specialty,
                        WorkingHours = consultant.WorkingHours ?? new List<DateTime>(),
                        User =
                            consultant.User != null
                                ? new UserDTO
                                {
                                    UserID = consultant.User.UserID,
                                    FullName = consultant.User.FullName,
                                    Username = consultant.User.Username,
                                    Email = consultant.User.Email,
                                }
                                : null,
                        CreatedCourses =
                            consultant
                                .User?.CreatedCourses?.Select(course => new CourseDto
                                {
                                    CourseID = course.CourseID,
                                    CourseName = course.Title,
                                    CourseDescription = course.Description,
                                    Status = course.isActive == true ? "Active" : "Inactive",
                                    CreatedByID = course.CreatedBy,
                                    CreatedAt = course.CreatedAt,
                                })
                                .ToList() ?? new List<CourseDto>(),
                    })
                    .ToList(),
            };
        }
    }
}
