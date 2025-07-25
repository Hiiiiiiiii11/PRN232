using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.CourseDashboard;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugUserPreventionUI.Models.Courses
{
    //// Course List DTO - Matching backend
    //public class CourseListDto
    //{
    //    public int CourseID { get; set; }
    //    public string Title { get; set; } = string.Empty;
    //    public string Description { get; set; } = string.Empty;
    //    public string TargetGroup { get; set; } = string.Empty;
    //    public string AgeGroup { get; set; } = string.Empty;
    //    public string ThumbnailURL { get; set; } = string.Empty; // URL của hình ảnh đại diện khóa học
    //    public string CreatorName { get; set; } = string.Empty;
    //    public DateTime CreatedAt { get; set; }
    //    public bool IsActive { get; set; }
    //    public bool IsAccept { get; set; }
    //    public int TotalContents { get; set; }
    //    public int TotalRegistrations { get; set; }
    //}

    // Course Detail DTO - Matching backend
    public class CourseResponseDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public string? ContentURL { get; set; }
        public string ThumbnailURL { get; set; } = string.Empty; // URL của hình ảnh đại diện khóa học
        public int? CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccept { get; set; }
        public int TotalContents { get; set; }
        public int TotalRegistrations { get; set; }
    }

    // Course Filter DTO - Matching backend structure
    public class CourseFilterDto : BasePaginationDto
    {
        public string? SearchKeyword { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public List<string>? Skills { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAccept { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Additional properties for UI
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    // Create Course DTO (for API calls)
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả khóa học là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhóm đối tượng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Nhóm đối tượng không được vượt quá 100 ký tự")]
        public string TargetGroup { get; set; } = string.Empty;

        [Required(ErrorMessage = "Độ tuổi là bắt buộc")]
        [StringLength(50, ErrorMessage = "Độ tuổi không được vượt quá 50 ký tự")]
        public string AgeGroup { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL nội dung không hợp lệ")]
        public string? ContentURL { get; set; }

        [Url(ErrorMessage = "URL hình đại diện không hợp lệ")]
        public string? ThumbnailURL { get; set; }
    }

    // Update Course DTO
    public class UpdateCourseDto
    {
        public int CourseID { get; set; }

        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả khóa học là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhóm đối tượng là bắt buộc")]
        public string TargetGroup { get; set; } = string.Empty;

        [Required(ErrorMessage = "Độ tuổi là bắt buộc")]
        public string AgeGroup { get; set; } = string.Empty;

        public string? ContentURL { get; set; }
        public string? ThumbnailURL { get; set; }
    }

    // Registration DTOs for Learning section
    public class CourseRegistrationResponseDto
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty; // Enrolled, InProgress, Completed, Dropped
        public CourseListDto? Course { get; set; }
        public UserSummaryDto? User { get; set; }
    }

    // User Summary DTO
    public class UserSummaryDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    // Registration Filter DTO
    public class RegistrationFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchKeyword { get; set; }
        public string? SortBy { get; set; } = "RegistrationDate";
        public bool SortDescending { get; set; } = true;
    }

    // Registration List DTO
    public class RegistrationListDto
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseThumbnail { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastAccessDate { get; set; }
    }

    // Update Progress DTO
    public class UpdateProgressDto
    {
        public int CourseID { get; set; }
        public int? ContentID { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    // Dashboard DTOs
    public class UserLearningDashboardDto
    {
        public int TotalRegistrations { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public decimal OverallProgress { get; set; }
        public List<RegistrationListDto>? RecentCourses { get; set; }
        public List<RegistrationListDto>? RecommendedCourses { get; set; }
        public LearningStatsDto? LearningStats { get; set; }
    }

    // Learning Stats DTO
    public class LearningStatsDto
    {
        public int TotalLearningHours { get; set; }
        public int TotalCompletedLessons { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastLearningDate { get; set; }
        public List<DailyLearningDto>? WeeklyProgress { get; set; }
    }

    // Daily Learning DTO
    public class DailyLearningDto
    {
        public DateTime Date { get; set; }
        public int MinutesLearned { get; set; }
        public int LessonsCompleted { get; set; }
    }

    // Course Enrollment Stats DTO
    public class CourseEnrollmentStatsDto
    {
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageProgress { get; set; }
        public List<MonthlyEnrollmentDto>? MonthlyTrends { get; set; }
        public List<EnrollmentByAgeGroupDto>? AgeGroupDistribution { get; set; }
    }

    // Monthly Enrollment DTO
    public class MonthlyEnrollmentDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Enrollments { get; set; }
        public int Completions { get; set; }
    }

    // Enrollment by Age Group DTO
    public class EnrollmentByAgeGroupDto
    {
        public string AgeGroup { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
