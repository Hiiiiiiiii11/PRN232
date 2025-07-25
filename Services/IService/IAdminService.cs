using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IAdminService
    {
        // User retrieval operations
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByStatusAsync(string status);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<List<User>> SearchUsersAsync(string searchTerm);
        Task<(List<User> Users, int TotalCount, int TotalPages)> GetUsersWithPaginationAsync(int page, int pageSize);

        // User creation and modification
        Task<(bool Success, string Message)> CreateUserAsync(User user);
        Task<(bool Success, string Message)> UpdateUserAsync(User user);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);

        // User status management
        Task<(bool Success, string Message)> BanUserAsync(int userId);
        Task<(bool Success, string Message)> UnbanUserAsync(int userId);
        Task<(bool Success, string Message)> ChangeUserRoleAsync(int userId, string newRole);
        Task<(bool Success, string Message)> VerifyUserEmailAsync(int userId);

        // Password management
        Task<(bool Success, string Message)> ResetUserPasswordAsync(int userId, string newPassword);
        Task<(bool Success, string Message)> ChangeUserPasswordAsync(int userId, string oldPassword, string newPassword);

        // Validation operations
        Task<bool> IsEmailAvailableAsync(string email);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> UserExistsAsync(int userId);

        // Statistics and reporting
        Task<int> GetTotalUsersCountAsync();
        Task<Dictionary<string, int>> GetUsersByRoleStatisticsAsync();
        Task<Dictionary<string, int>> GetUsersByStatusStatisticsAsync();
        Task<List<User>> GetRecentlyRegisteredUsersAsync(int count = 10);
    }
}